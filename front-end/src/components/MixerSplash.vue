<template>
  <div class="splash-wrapper">
    <canvas ref="canvasRef" class="splash-canvas" />

    <div class="splash-overlay">
      <h1 class="splash-title">WorshipHub<span class="splash-accent">Mixer</span></h1>
      <p class="splash-subtitle">Virtual Soundcheck</p>

      <transition name="fade-slide" mode="out-in">
        <!-- BOTÃO (Quando não está carregando) -->
        <button
          v-if="!isProcessing"
          key="btn"
          class="splash-btn"
          @click="$emit('start')"
          @mouseenter="isHovering = true"
          @mouseleave="isHovering = false"
        >
          <span class="btn-icon">▶</span>
          <span class="btn-text">Abrir Mixer</span>
        </button>

        <!-- CARREGAMENTO (Quando o usuário já clicou) -->
        <div v-else key="loader" class="loader-container">
          <p class="loading-stage">{{ loadingStage }}</p>
          <div class="progress-track">
            <div
              class="progress-bar"
              :style="{ width: loadProgress + '%' }"
            />
          </div>
          <div class="progress-text">{{ loadProgress }}%</div>
        </div>
      </transition>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount, watch } from 'vue'
import * as THREE from 'three'

const props = defineProps({
  isProcessing: { type: Boolean, default: false },
  loadProgress: { type: Number, default: 0 },
  loadingStage: { type: String, default: 'Iniciando Engine...' }
})

defineEmits(['start'])

const canvasRef = ref(null)
const isHovering = ref(false)

// === CONFIGURAÇÃO ===
const PARTICLE_COUNT = 2000
const SPHERE_RADIUS = 3
const MORPH_SPEED = 0.04 // Velocidade do lerp (0-1, quanto maior, mais rápido)
const ROTATION_SPEED = 0.003

let scene, camera, renderer, particles, animationId
let targetPositions = null   // Posições-alvo para morphing
let spherePositions = null   // Posições originais da esfera
let notePositions = null     // Posições da forma de nota musical

/**
 * Gera posições para uma esfera de partículas
 */
function generateSpherePositions(count, radius) {
  const positions = new Float32Array(count * 3)
  for (let i = 0; i < count; i++) {
    const phi = Math.acos(2 * Math.random() - 1)
    const theta = Math.random() * Math.PI * 2
    const r = radius * (0.8 + Math.random() * 0.4)

    positions[i * 3] = r * Math.sin(phi) * Math.cos(theta)
    positions[i * 3 + 1] = r * Math.sin(phi) * Math.sin(theta)
    positions[i * 3 + 2] = r * Math.cos(phi)
  }
  return positions
}

/**
 * Gera posições para uma nota musical (♪) feita de partículas
 * Usa formas geométricas simples: círculo (cabeça) + linha (haste) + curva (bandeira)
 */
function generateNotePositions(count, scale) {
  const positions = new Float32Array(count * 3)

  const headCount = Math.floor(count * 0.35)
  const stemCount = Math.floor(count * 0.30)
  const flagCount = Math.floor(count * 0.20)
  const scatterCount = count - headCount - stemCount - flagCount

  let idx = 0

  // Cabeça da nota (elipse inclinada na parte inferior)
  for (let i = 0; i < headCount; i++) {
    const angle = Math.random() * Math.PI * 2
    const r = Math.random() * 0.7
    positions[idx * 3] = (r * Math.cos(angle) * 1.2 - 0.3) * scale
    positions[idx * 3 + 1] = (r * Math.sin(angle) * 0.8 - 2.2) * scale
    positions[idx * 3 + 2] = (Math.random() - 0.5) * 0.3 * scale
    idx++
  }

  // Haste (linha vertical à direita da cabeça)
  for (let i = 0; i < stemCount; i++) {
    const t = Math.random()
    positions[idx * 3] = (0.5 + (Math.random() - 0.5) * 0.1) * scale
    positions[idx * 3 + 1] = (-2 + t * 4) * scale
    positions[idx * 3 + 2] = (Math.random() - 0.5) * 0.2 * scale
    idx++
  }

  // Bandeira (curva no topo da haste)
  for (let i = 0; i < flagCount; i++) {
    const t = Math.random()
    const curve = Math.sin(t * Math.PI) * 1.2
    positions[idx * 3] = (0.5 + curve * 0.8) * scale
    positions[idx * 3 + 1] = (2 - t * 1.8) * scale
    positions[idx * 3 + 2] = (Math.random() - 0.5) * 0.3 * scale
    idx++
  }

  // Partículas dispersas ao redor (ambiente)
  for (let i = 0; i < scatterCount; i++) {
    const angle = Math.random() * Math.PI * 2
    const r = 2.5 + Math.random() * 2
    positions[idx * 3] = Math.cos(angle) * r * scale * 0.5
    positions[idx * 3 + 1] = (Math.random() - 0.5) * 4 * scale
    positions[idx * 3 + 2] = Math.sin(angle) * r * scale * 0.3
    idx++
  }

  return positions
}

function initScene() {
  const canvas = canvasRef.value
  if (!canvas) return

  // Scene
  scene = new THREE.Scene()

  // Camera
  camera = new THREE.PerspectiveCamera(60, canvas.clientWidth / canvas.clientHeight, 0.1, 100)
  camera.position.z = 7

  // Renderer
  renderer = new THREE.WebGLRenderer({
    canvas,
    alpha: true,
    antialias: true
  })
  renderer.setSize(canvas.clientWidth, canvas.clientHeight)
  renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2))

  // Gerar geometrias
  spherePositions = generateSpherePositions(PARTICLE_COUNT, SPHERE_RADIUS)
  notePositions = generateNotePositions(PARTICLE_COUNT, 0.9)
  targetPositions = new Float32Array(spherePositions)

  // Geometria do sistema de partículas
  const geometry = new THREE.BufferGeometry()
  geometry.setAttribute('position', new THREE.BufferAttribute(new Float32Array(spherePositions), 3))

  // Cores por partícula (gradiente ciano → verde)
  const colors = new Float32Array(PARTICLE_COUNT * 3)
  for (let i = 0; i < PARTICLE_COUNT; i++) {
    const t = i / PARTICLE_COUNT
    colors[i * 3] = 0 + t * 0       // R: 0
    colors[i * 3 + 1] = 1 - t * 0.2 // G: 1.0 → 0.8
    colors[i * 3 + 2] = 0.53 + t * 0.47 // B: 0.53 → 1.0  (#00ff88 → #00ccff)
  }
  geometry.setAttribute('color', new THREE.BufferAttribute(colors, 3))

  // Material
  const material = new THREE.PointsMaterial({
    size: 0.04,
    vertexColors: true,
    transparent: true,
    opacity: 0.85,
    blending: THREE.AdditiveBlending,
    depthWrite: false
  })

  particles = new THREE.Points(geometry, material)
  scene.add(particles)

  // Resize handler
  window.addEventListener('resize', onResize)

  // Start animation loop
  animate()
}

function onResize() {
  const canvas = canvasRef.value
  if (!canvas || !renderer || !camera) return
  camera.aspect = canvas.clientWidth / canvas.clientHeight
  camera.updateProjectionMatrix()
  renderer.setSize(canvas.clientWidth, canvas.clientHeight)
}

function animate() {
  animationId = requestAnimationFrame(animate)

  if (!particles) return

  const positionAttr = particles.geometry.attributes.position
  const positions = positionAttr.array
  const target = (isHovering.value || props.isProcessing) ? notePositions : spherePositions

  // Interpolar partículas para a posição alvo (morphing suave)
  for (let i = 0; i < positions.length; i++) {
    positions[i] += (target[i] - positions[i]) * MORPH_SPEED
  }
  positionAttr.needsUpdate = true

  // Rotação suave
  particles.rotation.y += ROTATION_SPEED
  particles.rotation.x += ROTATION_SPEED * 0.3

  renderer.render(scene, camera)
}

function destroyScene() {
  if (animationId) cancelAnimationFrame(animationId)
  window.removeEventListener('resize', onResize)

  if (particles) {
    particles.geometry.dispose()
    particles.material.dispose()
    scene.remove(particles)
  }

  if (renderer) {
    renderer.dispose()
  }

  scene = null
  camera = null
  renderer = null
  particles = null
}

onMounted(() => {
  initScene()
})

onBeforeUnmount(() => {
  destroyScene()
})
</script>

<style scoped>
.splash-wrapper {
  position: relative;
  width: 100%;
  height: 100%;
  background: #1a1a2e;
  overflow: hidden;
}

.splash-canvas {
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
}

.splash-overlay {
  position: absolute;
  inset: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  z-index: 10;
  pointer-events: none;
}

.splash-title {
  font-size: 36px;
  font-weight: 300;
  letter-spacing: 4px;
  color: #ffffff;
  text-shadow: 0 0 40px rgba(0, 255, 136, 0.3);
  margin: 0;
  user-select: none;
}

.splash-accent {
  font-weight: 700;
  background: linear-gradient(135deg, #00ff88, #00ccff);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.splash-subtitle {
  font-size: 13px;
  letter-spacing: 6px;
  text-transform: uppercase;
  color: rgba(255, 255, 255, 0.4);
  margin-top: 6px;
  margin-bottom: 48px;
  user-select: none;
}

.splash-btn {
  pointer-events: all;
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 14px 36px;
  border: 1px solid rgba(0, 255, 136, 0.3);
  border-radius: 50px;
  background: rgba(0, 255, 136, 0.06);
  color: #00ff88;
  font-size: 15px;
  font-weight: 600;
  letter-spacing: 1px;
  cursor: pointer;
  transition: all 0.4s cubic-bezier(0.16, 1, 0.3, 1);
  backdrop-filter: blur(8px);
}

.splash-btn:hover {
  background: rgba(0, 255, 136, 0.15);
  border-color: rgba(0, 255, 136, 0.6);
  box-shadow:
    0 0 30px rgba(0, 255, 136, 0.2),
    0 0 60px rgba(0, 204, 255, 0.1);
  transform: scale(1.05);
}

.splash-btn:active {
  transform: scale(0.98);
}

.btn-icon {
  font-size: 18px;
}

.btn-text {
  text-transform: uppercase;
}

/* Transições Splash -> Loading */
.fade-slide-enter-active,
.fade-slide-leave-active {
  transition: all 0.4s ease;
}
.fade-slide-enter-from {
  opacity: 0;
  transform: translateY(10px);
}
.fade-slide-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

/* Loader Estilos Novos */
.loader-container {
  width: 320px;
  text-align: center;
  margin-top: 10px;
}

.loading-stage {
  font-size: 14px;
  color: #00ccff;  /* Ciano claro combinando com a cena */
  margin-bottom: 16px;
  letter-spacing: 1px;
}

.progress-track {
  height: 4px;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 4px;
  overflow: hidden;
}

.progress-bar {
  height: 100%;
  background: linear-gradient(90deg, #00ff88, #00ccff);
  transition: width 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  box-shadow: 0 0 10px rgba(0, 255, 136, 0.5);
}

.progress-text {
  margin-top: 14px;
  font-size: 13px;
  color: #00ff88;
  font-weight: 600;
  letter-spacing: 1px;
}
</style>
