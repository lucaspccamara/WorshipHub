<template>
  <canvas ref="canvas" class="ruler-canvas"></canvas>
</template>

<script setup>
import { onMounted, ref, watch } from 'vue'

const props = defineProps({
  width: { type: Number, default: 30 },
  height: { type: Number, default: 280 }
})

const canvas = ref(null)

function dbToRatio(db) {
  const norm = (db + 60) / 66 
  return Math.pow(Math.max(0, Math.min(1, norm)), 2)
}

function draw() {
  const c = canvas.value
  if (!c) return
  const ctx = c.getContext('2d')
  
  // Resolução para Retina/Telas de alta densidade
  const dpr = window.devicePixelRatio || 1
  c.width = props.width * dpr
  c.height = props.height * dpr
  ctx.scale(dpr, dpr)

  ctx.clearRect(0, 0, props.width, props.height)
  
  // Padding vertical para não cortar o texto nas extremidades (+6 e -60)
  const paddingV = 10
  const drawH = props.height - (paddingV * 2) // 260px de área útil

  // Estilo da Régua
  ctx.textAlign = 'right'
  ctx.textBaseline = 'middle'
  ctx.font = '600 10px "Inter", sans-serif'
  ctx.fillStyle = 'rgba(255, 255, 255, 0.3)'

  const marks = [6, 0, -6, -12, -18, -24, -30, -36, -42, -48, -60]
  
  marks.forEach(db => {
    const ratio = dbToRatio(db)
    // Posicionamento com offset de paddingV
    const y = paddingV + (drawH - (ratio * drawH))
    
    // Desenhar Número
    ctx.fillText(db.toString(), props.width - 8, y)
    
    // Desenhar Tick (linha pequena)
    ctx.strokeStyle = 'rgba(255, 255, 255, 0.1)'
    ctx.lineWidth = 1
    ctx.beginPath()
    ctx.moveTo(props.width - 5, y)
    ctx.lineTo(props.width, y)
    ctx.stroke()
  })
}

onMounted(() => {
  draw()
})

// Redesenha se o tamanho mudar
watch(() => [props.width, props.height], draw)
</script>

<style scoped>
.ruler-canvas {
  width: 30px;
  height: 280px;
  display: block;
  user-select: none;
  pointer-events: none;
}
</style>
