<template>
  <div class="splash-wrapper">
    <canvas ref="canvasRef" class="splash-canvas" />

    <div class="splash-overlay">
      <h1 class="splash-title">WorshipHub<span class="splash-accent">Mixer</span></h1>

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
import { SVGLoader } from 'three/examples/jsm/loaders/SVGLoader'

const props = defineProps({
  isProcessing: { type: Boolean, default: false },
  loadProgress: { type: Number, default: 0 },
  loadingStage: { type: String, default: 'Iniciando Engine...' }
})

defineEmits(['start'])

const canvasRef = ref(null)
const isHovering = ref(false)

// === CONFIGURAÇÃO ===
const PARTICLE_COUNT = 5000
const SPHERE_RADIUS = 3
const MORPH_SPEED = 0.04 
const ROTATION_SPEED = 0.0015

let scene, camera, renderer, particles, animationId
let spherePositions = null   
let notePositions = null     
let targetPositions = null   
let svgPaths = [] 
let svgCenter = { x: 0, y: 0 }
let svgTargetScale = 0.01
const USER_SCALE_ADJUST = 1.0 // Escala 1.0 = Centralizado e Visível

/**
 * Gera posições para uma esfera de partículas
 */
function generateSpherePositions(count, radius) {
  const totalMainParticles = Math.floor(count * 0.5)
  let idx = 0
  const positions = new Float32Array(count * 3)

  for (let i = 0; i < totalMainParticles; i++) {
    const phi = Math.acos(2 * Math.random() - 1)
    const theta = Math.random() * Math.PI * 2
    
    // Dispersão maior: as partículas variam entre o raio base e o dobro do raio
    // para criar uma nebulosa mais etérea e menos concentrada.
    const r = radius * (0.8 + Math.random() * 0.8) 

    positions[i * 3] = r * Math.sin(phi) * Math.cos(theta)
    positions[i * 3 + 1] = r * Math.sin(phi) * Math.sin(theta)
    positions[i * 3 + 2] = r * Math.cos(phi)
    idx++
  }

  // Restante das partículas: ESFERA FANTASMA
  const remaining = count - totalMainParticles
  for (let i = 0; i < remaining; i++) {
    const phi = Math.acos(2 * Math.random() - 1)
    const theta = Math.random() * Math.PI * 2
    // Reduzi o raio para 4-6 para não "engolir" a câmera que está em Z=7
    const r = (2 + Math.random() * 2) * radius
    
    positions[idx * 3] = r * Math.sin(phi) * Math.cos(theta)
    positions[idx * 3 + 1] = r * Math.sin(phi) * Math.sin(theta)
    positions[idx * 3 + 2] = r * Math.cos(phi)
    idx++
  }

  return positions
}

/**
 * Gera as posições baseadas no arquivo SVG carregado
 */
function generateNotePositions(count, scale) {
  const positions = new Float32Array(count * 3)
  let idx = 0

  if (svgPaths.length > 0) {
    const totalMainParticles = Math.floor(count * 0.85)
    
    // Coletar todos os subPaths e seus comprimentos
    let totalLength = 0
    const allSubPaths = []
    svgPaths.forEach(path => {
      path.subPaths.forEach(sub => {
        const len = sub.getLength()
        totalLength += len
        allSubPaths.push({ sub, len })
      })
    })

    // Amostrar pontos
    allSubPaths.forEach(({ sub, len }) => {
      const particlesForPath = Math.floor((len / totalLength) * totalMainParticles)
      if (particlesForPath < 1) return

      const points = sub.getSpacedPoints(particlesForPath)
      points.forEach(p => {
        if (idx < count) {
          // Centralização por Bounding Box real e Escala Dinâmica
          positions[idx * 3] = (p.x - svgCenter.x) * svgTargetScale * USER_SCALE_ADJUST * scale
          positions[idx * 3 + 1] = (p.y - svgCenter.y) * -svgTargetScale * USER_SCALE_ADJUST * scale
          positions[idx * 3 + 2] = (Math.random() - 0.5) * 0.2 * scale
          idx++
        }
      })
    })
  }

  // Restante das partículas: ESFERA FANTASMA
  const remaining = count - idx
  for (let i = 0; i < remaining; i++) {
    const phi = Math.acos(2 * Math.random() - 1)
    const theta = Math.random() * Math.PI * 2
    // Reduzi o raio para 4-6 para não "engolir" a câmera que está em Z=7
    const r = (4 + Math.random() * 2) * scale
    
    positions[idx * 3] = r * Math.sin(phi) * Math.cos(theta)
    positions[idx * 3 + 1] = r * Math.sin(phi) * Math.sin(theta)
    positions[idx * 3 + 2] = r * Math.cos(phi)
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

  // Rotação condicional
  const isMusicMode = isHovering.value || props.isProcessing
  if (isMusicMode) {
    // Modo Nota: Travar X e Z, parar rotação Y
    particles.rotation.x += (0 - particles.rotation.x) * 0.08
    particles.rotation.y += (0 - particles.rotation.y) * 0.08
    particles.rotation.z += (0 - particles.rotation.z) * 0.08
    
    // Pequeno movimento de flutuação (viva) para as notas
    const time = Date.now() * 0.001
    particles.position.y = Math.sin(time * 0.5) * 0.1
    particles.position.x = Math.cos(time * 0.3) * 0.05
  } else {
    // Modo Esfera: Rotação livre nos 3 eixos e reset de posição
    particles.rotation.y += ROTATION_SPEED
    particles.rotation.x += ROTATION_SPEED * 0.3
    particles.rotation.z += ROTATION_SPEED * 0.2
    
    particles.position.y += (0 - particles.position.y) * 0.05
    particles.position.x += (0 - particles.position.x) * 0.05
  }

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
  // Inicializa o loader de SVG para extrair os paths
  const loader = new SVGLoader()
  loader.load(
    '/mixer-note-shape.svg', 
    (data) => {
      svgPaths = data.paths

      // CÁLCULO DE BOUNDING BOX REAL
      let minX = Infinity, minY = Infinity, maxX = -Infinity, maxY = -Infinity
      data.paths.forEach(path => {
        path.subPaths.forEach(sub => {
          const pts = sub.getPoints(5)
          pts.forEach(p => {
            if (p.x < minX) minX = p.x; if (p.x > maxX) maxX = p.x
            if (p.y < minY) minY = p.y; if (p.y > maxY) maxY = p.y
          })
        })
      })

      svgCenter.x = (minX + maxX) / 2
      svgCenter.y = (minY + maxY) / 2
      const w = maxX - minX
      const h = maxY - minY
      
      // Escala base para caber na câmera, multiplicada pelo USER_SCALE_ADJUST
      svgTargetScale = 10 / Math.max(w, h)

      // Regenerar
      notePositions = generateNotePositions(PARTICLE_COUNT, 1)
    }
  )

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
  color: #ffffff;  /* Ciano claro combinando com a cena */
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
  color: #ffffff;
  font-weight: 600;
  letter-spacing: 1px;
}
</style>
