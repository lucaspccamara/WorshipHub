<template>
  <div class="mixer-wrapper">

    <!-- SPLASH & LOADING UNIFICADOS (Gerenciado agora pelo MixerSplash) -->
    <transition name="fade-slow">
      <MixerSplash
        v-if="mostrarSplashOuLoading"
        :isProcessing="isLoading"
        :loadProgress="loadProgress"
        :loadingStage="loadingStage"
        @start="startMixer"
      />
    </transition>

    <transition name="fade-slow">
      <div v-show="!mostrarSplashOuLoading" class="mixer-screen">
        <div 
          v-for="track in tracks" 
          :key="track.name" 
          class="channel" 
          :class="{ 
            'is-muted': track.mute || (hasAnySolo && !track.solo), 
            'is-solo': track.solo 
          }"
        >

          <!-- DISPLAY DB -->
          <div class="db-display">
            {{ track.db.toFixed(1) }}
          </div>

          <!-- STRIP (Escala + Fader + Meter) -->
          <div class="strip">
            <!-- SCALE -->
            <div class="scale">
              <div v-for="mark in dbMarks" :key="mark">
                {{ mark }}
              </div>
            </div>

            <!-- FADER CONTAINER (com nossa lógica Custom de Drag/Daw) -->
            <div class="fader-wrapper">
              <q-slider
                vertical
                reverse
                :min="-60"
                :max="6"
                :step="0.1"
                v-model="track.db"
                class="fader q-slider-cyan"
                track-size="4px"
                thumb-size="16px"
                :color="track.solo ? 'orange-8' : (track.mute || (hasAnySolo && !track.solo) ? 'grey-7' : 'cyan-4')"
                readonly
              />
              
              <!-- ÁREA INVISÍVEL DE CAPTURA DE MOVES -->
              <div 
                class="fader-overlay"
                @mousedown="startDrag($event, track)"
                @touchstart.prevent="startDrag($event, track)"
                @dblclick="resetDb(track)"
              ></div>
            </div>

            <!-- METER -->
            <div class="meter">
              <MeterCanvas :analyser="track.analyser" />
            </div>
          </div>

          <!-- CHANNEL NAME (Mute Button) -->
          <div 
            class="btn-name" 
            :class="{ active: !track.mute }" 
            @click="toggleMute(track)"
          >
            {{ track.name }}
          </div>

          <!-- SOLO BUTTON -->
          <div class="buttons">
            <div
              class="btn-solo"
              :class="{ active: track.solo }"
              @click="toggleSolo(track)"
            >
              S
            </div>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, watch, computed } from 'vue'
import { useAudioMixer } from '../composables/useAudioMixer'
import MeterCanvas from './MeterCanvas.vue'
import MixerSplash from './MixerSplash.vue'

const props = defineProps({
  musicId: {
    type: Number,
    required: true
  }
})

const {
  tracks,
  isLoading,
  loadingStage,
  loadProgress,
  loadTracks,
  setDb,
  toggleMute,
  toggleSolo
} = useAudioMixer()

// Identifica se existe algum canal em modo Solo na mesa
const hasAnySolo = computed(() => tracks.value.some(t => t.solo))

// Controle unificado: exibe o splash (com o botão ou com a barra de loading ativada)
// Só vira `false` quando o carregamento terminar, ou se as tracks já estiverem carregadas na memória.
const mostrarSplashOuLoading = ref(tracks.value.length === 0)

/**
 * Busca as tracks da música selecionada.
 * No modo mock: lê /mock/{musicId}/tracks.json
 * Futuramente: GET /api/musics/{id}/tracks
 */
async function fetchTracks(musicId) {
  const basePath = `/mock/${musicId}`
  const response = await fetch(`${basePath}/tracks.json`)

  if (!response.ok) {
    console.error(`Tracks não encontradas para a música ${musicId}`)
    return []
  }

  try {
    const manifest = await response.json()
    return manifest.map(t => ({
      name: t.name,
      url: `${basePath}/${t.file}`,
      order: t.order
    }))
  } catch (error) {
    console.error(`Erro ao parsear tracks.json (provavelmente o arquivo não existe e a rota retornou HTML):`, error)
    return []
  }
}

/**
 * LOGICA DE DRAG SUAVE (Estilo DAW)
 * Intercepta o movimento do mouse para não "pular" pro local clicado, apenas arrastar a partir do ponto clicado.
 */
let dragInitialY = 0
let dragInitialDb = 0
let currentDragTrack = null
const SENSITIVITY = 0.15 // Visualmente ajustado para cobrir ~55% da escala ao percorrer toda a extensão física do fader (≈15dB a cada 100px).

function startDrag(event, track) {
  currentDragTrack = track
  dragInitialY = event.clientY || (event.touches ? event.touches[0].clientY : 0)
  dragInitialDb = track.db
  
  // Prevenir seleção de texto em toda a tela durante o arrasto
  document.body.style.userSelect = 'none'

  window.addEventListener('mousemove', onDrag)
  window.addEventListener('mouseup', stopDrag)
  window.addEventListener('touchmove', onDrag, { passive: false })
  window.addEventListener('touchend', stopDrag)
}

function onDrag(event) {
  if (!currentDragTrack) return
  event.preventDefault()
  
  const clientY = event.clientY || (event.touches ? event.touches[0].clientY : 0)
  const deltaY = clientY - dragInitialY
  
  // Como o slider é vertical com "reverse" (-60 em baixo, 6 em cima):
  // Mover o mouse para baixo (deltaY positivo) diminui o volume.
  let newDb = dragInitialDb - (deltaY * SENSITIVITY)
  
  if (newDb > 6) newDb = 6
  if (newDb < -60) newDb = -60
  
  currentDragTrack.db = newDb
  setDb(currentDragTrack, newDb)
}

function stopDrag() {
  currentDragTrack = null
  
  // Restaurar seleção de texto
  document.body.style.userSelect = ''

  window.removeEventListener('mousemove', onDrag)
  window.removeEventListener('mouseup', stopDrag)
  window.removeEventListener('touchmove', onDrag)
  window.removeEventListener('touchend', stopDrag)
}

function resetDb(track) {
  track.db = 0
  setDb(track, 0)
}

/**
 * Inicia o carregamento do mixer quando o usuário clica no botão do Splash
 */
async function startMixer() {
  const trackFiles = await fetchTracks(props.musicId)
  if (trackFiles.length > 0) {
    loadTracks(trackFiles)
  }
}

// Escuta o fim do carregamento para esconder a tela de Splash/Loading e revelar o Mixer
watch(isLoading, (loading, oldLoading) => {
  // Se está mudando de "carregando" (true) para "terminou" (false)
  if (oldLoading === true && loading === false) {
    // Atraso intencional mínimo para suavizar a UX de 100% para sumiço
    setTimeout(() => {
      mostrarSplashOuLoading.value = false
    }, 400)
  }
})

const dbMarks = [6, 0, -6, -12, -18, -24, -30, -36, -42, -48, -54, -60]
</script>

<style scoped>
.mixer-wrapper {
  position: relative;
  height: 100%;
  background: #1a1a2e; /* Mantém a paleta da splash */
  color: #eee;
  overflow: hidden;
}

/* Transição lenta e elegante */
.fade-slow-enter-active,
.fade-slow-leave-active {
  transition: opacity 0.8s ease-in-out;
}
.fade-slow-enter-from,
.fade-slow-leave-to {
  opacity: 0;
}

/* === MIXER SCREEN (DESIGN PREMIUM) === */
.mixer-screen {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: flex-start;
  overflow-x: auto;
  gap: 12px;
  padding: 24px;
  background: radial-gradient(circle at center, #1b1b22 0%, #101018 100%);
}

/* Canais com Glassmorphism Neon sutil */
.channel {
  width: 104px;
  background: rgba(255, 255, 255, 0.02);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
  border: 1px solid rgba(0, 255, 136, 0.05);
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.2);
  padding: 16px 10px;
  display: flex;
  flex-direction: column;
  align-items: center;
  border-radius: 12px;
  transition: border-color 0.3s ease;
}

.channel:hover {
  border-color: rgba(0, 255, 136, 0.15);
  background: rgba(255, 255, 255, 0.03);
}

/* Visor Digital de dB */
.db-display {
  background: #0d0d15;
  border: 1px solid #222;
  box-shadow: inset 0 2px 4px rgba(0,0,0,0.5);
  padding: 6px 0;
  border-radius: 6px;
  font-family: 'Courier New', Courier, monospace;
  font-variant-numeric: tabular-nums; /* Evita pulos baseados nos caracteres */
  font-size: 13px;
  font-weight: 600;
  color: #00ccff;
  letter-spacing: 1px;
  margin-bottom: 30px;
  width: 50px; /* Largura mínima e fixa para comportar "-60.0" sem quebrar */
  text-align: center;
}

.strip {
  display: flex;
  height: 260px;
  align-items: flex-end;
  position: relative;
  width: 100%;
  justify-content: center;
}

/* Escala */
.scale {
  font-size: 10px;
  color: rgba(255, 255, 255, 0.3);
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  text-align: right;
  padding-right: 8px;
  user-select: none;
}

/* Área de Custom Drag */
.fader-wrapper {
  position: relative;
  height: 100%;
  width: 24px;
  margin-right: 12px;
}

.fader {
  height: 100%;
  width: 100%;
  pointer-events: none; /* Deixa o Quasar apenas de mostruário */
}

/* Camada invisível que de fato apanha os cliques e drags do mouse */
.fader-overlay {
  position: absolute;
  inset: -10px; /* Margem gorda em volta do slider para facilitar pegar com o dedo/mouse */
  z-index: 20;
  cursor: pointer; /* Cursor padrão de botão (mãozinha) */
  touch-action: none; /* Previne rolagem da tela no mobile ao arrastar fader */
}

.fader-overlay:active {
  cursor: grabbing; /* Muda para "mão fechada" indicando que está segurando */
}

.meter {
  width: 14px;
  height: 260px;
  background: rgba(0,0,0, 0.6);
  border: 1px solid rgba(255,255,255, 0.05);
  position: relative;
  border-radius: 4px;
  overflow: hidden;
}

/* Botão de Nome do Canal (Mute) */
.btn-name {
  width: 100%;
  margin-top: 30px;
  padding: 8px 4px;
  font-size: 13px;
  font-weight: 600;
  color: rgba(255, 255, 255, 0.4);
  background: rgba(0, 0, 0, 0.3);
  border: 1px solid rgba(255, 255, 255, 0.05);
  border-radius: 6px;
  text-align: center;
  cursor: pointer;
  transition: all 0.3s ease;
  user-select: none;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.btn-name.active {
  color: #111;
  background: linear-gradient(135deg, #00ff88, #00ccff);
  box-shadow: 0 0 15px rgba(0, 255, 136, 0.3);
  border-color: transparent;
}

/* Botão Solo (S) */
.buttons {
  margin-top: 10px;
  display: flex;
  width: 100%;
  justify-content: center;
}

.btn-solo {
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  font-weight: 700;
  color: rgba(255, 255, 255, 0.3);
  background: rgba(0, 0, 0, 0.3);
  border: 1px solid rgba(255, 255, 255, 0.05);
  border-radius: 50%;
  cursor: pointer;
  transition: all 0.2s ease;
  user-select: none;
}

.btn-solo.active {
  color: #fff;
  background: #ff5e00;
  border-color: #ffaa00;
  box-shadow: 0 0 15px rgba(255, 94, 0, 0.5);
}

/* === FADER CUSTOMIZADO (MESA DE SOM REAL) === */
/* Remove TOTALMENTE as transições nativas do Quasar quando alteramos o v-model do script (evitando catch-up lag) */
:deep(.q-slider-cyan),
:deep(.q-slider-cyan *) {
  transition: none !important;
}

/* Oculta a bolinha padrão mas a mantém lá para acessibilidade */
:deep(.q-slider-cyan .q-slider__thumb) {
  opacity: 1 !important;
  width: 26px !important;
  height: 40px !important;
  border-radius: 4px !important;
  background: linear-gradient(180deg, #333 0%, #222 50%, #151515 100%) !important;
  border: 1px solid #000 !important;
  box-shadow: 
    0 5px 10px rgba(0,0,0,0.8), /* Sombra projetada */
    inset 0 1px 1px rgba(255,255,255,0.2), /* Brilho no topo */
    inset 0 -1px 1px rgba(0,0,0,0.8) !important; /* Escurecimento na base */
  position: absolute !important;
  left: 50% !important;
  /* Meu thumb agora tem 40px. 
     A diferença de centro a centro é 40/2 = 20px! Desloco para baixo: */
  transform: translate(-50%, 20px) !important;
}

/* O SVG dentro do thumb original (removeremos para ficar só nosso retangulo) */
:deep(.q-slider-cyan .q-slider__thumb svg) {
  display: none !important;
}

/* A linha central (brilhante/indicadora) do Fader físico */
:deep(.q-slider-cyan .q-slider__thumb::after) {
  content: '';
  position: absolute;
  top: 50%;
  left: 0;
  width: 100%;
  height: 3px;
  background: #00ccff; /* Cor indicadora alinhada ao neon */
  box-shadow: 0 0 5px #00ff88, inset 0 1px 1px rgba(255,255,255,0.5);
  border-top: 1px solid #000;
  border-bottom: 1px solid #000;
  transform: translateY(-50%);
}

/* Remover o anel transparente padrão de focus/hover do Quasar */
:deep(.q-slider-cyan .q-slider__focus-ring) {
  display: none !important;
}

/* === ESTADOS VISUAIS (MUTE E SOLO) === */

/* Quando Muted: escurece a trilha, a linha do fader e desliga o neon */
.channel.is-muted .strip,
.channel.is-muted .db-display {
  opacity: 0.4;
  transition: opacity 0.3s ease;
}

.channel.is-muted :deep(.q-slider-cyan .q-slider__thumb::after) {
  background: #555 !important;
  box-shadow: none !important;
  border-top: 1px solid #333;
  border-bottom: 1px solid #333;
}

/* Quando Solo: Coloração Laranja, independente do Mute */
.channel.is-solo {
  border-color: rgba(255, 94, 0, 0.4);
  background: rgba(255, 94, 0, 0.05);
}

.channel.is-solo .strip,
.channel.is-solo .db-display {
  opacity: 1 !important; /* Ignora o escurecimento do Mute se solado */
}

.channel.is-solo :deep(.q-slider-cyan .q-slider__thumb::after) {
  background: #ffcc00 !important;
  box-shadow: 0 0 8px #ff5e00, inset 0 1px 1px rgba(255,255,255,0.8) !important;
}
</style>