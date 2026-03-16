<template>
  <canvas ref="canvas" class="meter-canvas"></canvas>
</template>

<script setup>
import { onMounted, onBeforeUnmount, ref } from 'vue'

const props = defineProps({
  analyser: Object
})

const canvas = ref(null)
let ctx
let animationId
let dataArray
let meterValue = -60
let peakHoldValue = -60
let peakHoldTimer = 0

function linearToDb(value) {
  if (value <= 0.000001) return -60
  return 20 * Math.log10(value)
}

function draw() {
  if (!props.analyser) return

  props.analyser.getFloatTimeDomainData(dataArray)

  let sum = 0
  let peak = 0

  for (let i = 0; i < dataArray.length; i++) {
    const sample = Math.abs(dataArray[i])
    sum += sample * sample
    if (sample > peak) peak = sample
  }

  const rms = Math.sqrt(sum / dataArray.length)

  const rmsDb = linearToDb(rms)
  const peakDb = linearToDb(peak)

  // Combine RMS + Peak
  const targetDb = Math.max(rmsDb, peakDb - 3)

  // Ballistics estilo DAW
  const attack = 1
  const release = 0.08

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

  const normalized = (meterValue + 60) / 66
  const meterHeight = Math.max(0, normalized) * height

  const gradient = ctx.createLinearGradient(0, 0, 0, height)
  gradient.addColorStop(0, '#ff3300')
  gradient.addColorStop(0.25, '#ffff00')
  gradient.addColorStop(1, '#00ff66')

  ctx.fillStyle = gradient
  ctx.fillRect(0, height - meterHeight, width, meterHeight)

  // Desenhar Peak Hold
  const peakNorm = (peakHoldValue + 60) / 66
  const peakY = height - (peakNorm * height)

  ctx.fillStyle = '#ffffff'
  ctx.fillRect(0, peakY, width, 2)

  animationId = requestAnimationFrame(draw)
}

onMounted(() => {
  const c = canvas.value
  c.width = 14
  c.height = 260

  ctx = c.getContext('2d')

  dataArray = new Float32Array(props.analyser.fftSize)

  draw()
})

onBeforeUnmount(() => {
  cancelAnimationFrame(animationId)
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