<template>
  <canvas ref="canvas" class="meter-canvas"></canvas>
</template>

<script setup>
import { onMounted, onBeforeUnmount, ref } from 'vue'

const props = defineProps({
  meterNode: Object,
  analyser: Object
})

const canvas = ref(null)
const V_PADDING = 10 // Movido para o topo do setup (Escopo Geral)

let ctx
let animationId
let meterValue = -60
let peakHoldValue = -60
let peakHoldTimer = 0
let gradient = null
let isVisible = true
let observer = null

// Fallback para AnalyserNode caso Worklet falhe
let dataArray = null 

// Valores recebidos do Worklet ou extraídos do Analyser
let latestPeak = 0
let latestRms = 0
let lastPeakFallback = 0 // Para técnica de Peak Stretching no mobile

const isMobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)

function linearToDb(value) {
  if (value <= 0.001) return -60 
  return 20 * Math.log10(value)
}

function draw() {
  if (!isVisible) {
    animationId = requestAnimationFrame(draw)
    return
  }

  // LÓGICA DE EXTRAÇÃO DE DADOS (Worklet vs Analyser)
  if (props.meterNode) {
    // Valores já foram atualizados via onmessage
  } else if (props.analyser && dataArray) {
    // Processamento de Fallback (Impactado pelo FPS da tela)
    props.analyser.getFloatTimeDomainData(dataArray)
    let sum = 0
    let peak = 0
    for (let i = 0; i < dataArray.length; i++) {
      const s = Math.abs(dataArray[i])
      if (s > peak) peak = s
      if (i % 4 === 0) sum += s * s
    }
    
    // TÉCNICA DE PEAK STRETCHING (Mobile Only)
    // Se estivermos no mobile (geralmente 30fps), um pico pode ser rápido demais para o olho ou para o peakHold.
    // Combinamos o pico atual com o anterior para "alargar" sua duração visual.
    latestPeak = isMobile ? Math.max(peak, lastPeakFallback) : peak
    lastPeakFallback = peak // Guarda para o próximo frame
    
    latestRms = Math.sqrt(sum / (dataArray.length / 4))
  }

  // Converte os valores lineares recebidos do áudio para dB
  const rmsDb = linearToDb(latestRms)
  const peakDb = linearToDb(latestPeak)

  // O alvo visual do medidor
  const targetDb = Math.max(rmsDb, peakDb)

  // Ballistics adaptativa: No mobile, precisamos que a descida seja um pouco mais lenta
  // para compensar o atraso de desenho e tornar o pico mais visível.
  const attack = 1.0
  const release = isMobile ? 0.05 : 0.08
  if (targetDb > meterValue) {
    meterValue += (targetDb - meterValue) * attack
  } else {
    meterValue += (targetDb - meterValue) * release
  }

  // Peak Hold
  if (peakDb > peakHoldValue) {
    peakHoldValue = peakDb
    peakHoldTimer = performance.now()
  }
  if (performance.now() - peakHoldTimer > 800) {
    peakHoldValue -= 0.5
  }

  const height = canvas.value.height
  const width = canvas.value.width
  ctx.clearRect(0, 0, width, height)

  const normalizedDb = (meterValue + 60) / 66
  const normalized = Math.pow(Math.max(0, normalizedDb), 2)
  const meterHeight = normalized * height

  // Desenha o gradiente de volume
  ctx.fillStyle = gradient
  ctx.fillRect(0, height - meterHeight, width, meterHeight)

  // Desenhar linha de Peak Hold
  const peakHoldNormDb = (peakHoldValue + 60) / 66
  const peakNorm = Math.pow(Math.max(0, peakHoldNormDb), 2)
  const peakY = height - (peakNorm * height)

  ctx.fillStyle = '#ffffff'
  ctx.fillRect(0, peakY, width, 2)

  animationId = requestAnimationFrame(draw)
}

onMounted(() => {
  const c = canvas.value
  c.width = 14
  c.height = 260

  ctx = c.getContext('2d', { alpha: true })

  // PRÉ-RENDERIZAR GRADIENTE
  gradient = ctx.createLinearGradient(0, V_PADDING, 0, c.height - V_PADDING)
  gradient.addColorStop(0, '#ff3300')
  gradient.addColorStop(0.25, '#ffff00')
  gradient.addColorStop(1, '#00ff66')

  // ESCUTA O AUDIO WORKLET (Se disponível)
  if (props.meterNode) {
    props.meterNode.port.onmessage = (e) => {
      latestPeak = e.data.peak
      latestRms = e.data.rms
    }
  } else if (props.analyser) {
    // Configura Fallback
    dataArray = new Float32Array(props.analyser.fftSize)
  }

  // MONITOR DE VISIBILIDADE
  observer = new IntersectionObserver((entries) => {
    isVisible = entries[0].isIntersecting
  }, { threshold: 0.1 })
  
  if (c) observer.observe(c)

  draw()
})

onBeforeUnmount(() => {
  if (animationId) cancelAnimationFrame(animationId)
  if (observer) observer.disconnect()
  if (props.meterNode) {
    props.meterNode.port.onmessage = null
  }
})
</script>

<style scoped>
.meter-canvas {
  width: 14px;
  height: 260px;
  background: #111;
  border-radius: 2px;
}
</style>