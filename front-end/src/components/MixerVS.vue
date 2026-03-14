<template>
  <div class="mixer-wrapper">

    <!-- ESTADO 1: SPLASH (tela inicial) -->
    <transition name="fade">
      <MixerSplash
        v-if="mixerState === 'splash'"
        @start="startMixer"
      />
    </transition>

    <!-- ESTADO 2: LOADING (processando áudio) -->
    <transition name="fade">
      <div v-if="mixerState === 'loading'" class="loading-screen">
        <div class="loading-content">

          <h1 class="project-title">WorshipHub Mixer</h1>
          <p class="loading-stage">{{ loadingStage }}</p>

          <div class="progress-track">
            <div
              class="progress-bar"
              :style="{ width: loadProgress + '%' }"
            />
          </div>

          <div class="progress-text">
            {{ loadProgress }}%
          </div>

        </div>
      </div>
    </transition>

    <!-- ESTADO 3: MIXER (mesa de som) -->
    <transition name="fade">
      <div v-if="mixerState === 'mixer'" class="mixer-screen">
        <div v-for="track in tracks" :key="track.name" class="channel">

          <!-- DISPLAY -->
          <div class="db-display">
            {{ track.db.toFixed(1) }}
          </div>

          <!-- FADER + METER SIDE BY SIDE -->
          <div class="strip">

            <!-- SCALE -->
            <div class="scale">
              <div v-for="mark in dbMarks" :key="mark">
                {{ mark }}
              </div>
            </div>

            <!-- FADER -->
            <q-slider
              vertical
              reverse
              :min="-60"
              :max="6"
              :step="0.1"
              v-model="track.db"
              @update:model-value="val => setDb(track, val)"
              class="fader"
            />

            <!-- METER -->
            <div class="meter">
              <MeterCanvas :analyser="track.analyser" />
            </div>

          </div>

          <!-- CHANNEL NAME -->
          <div class="btn channel-name" :class="{ active: !track.mute }" @click="toggleMute(track)">
            {{ track.name }}
          </div>

          <!-- BUTTONS -->
          <div class="buttons">
            <div
              class="btn"
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
import { ref, watch } from 'vue'
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

const mixerState = ref('splash') // 'splash' | 'loading' | 'mixer'

/**
 * Busca as tracks da música selecionada.
 * No modo mock: lê /mock/{musicId}/tracks.json
 * Futuramente: GET /api/musics/{id}/tracks
 */
async function fetchTracks(musicId) {
  // TODO: Substituir por chamada à API quando o backend estiver pronto
  // const response = await api.get('musics', `${musicId}/tracks`)
  // return response.data.map(t => ({ name: t.name, url: t.fileUrl, order: t.order }))

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
 * Inicia o carregamento do mixer quando o usuário clica no botão
 */
async function startMixer() {
  mixerState.value = 'loading'
  const trackFiles = await fetchTracks(props.musicId)
  if (trackFiles.length > 0) {
    loadTracks(trackFiles)
  }
}

// Quando o loading termina, transitar para o mixer
watch(isLoading, (loading) => {
  if (!loading && mixerState.value === 'loading') {
    mixerState.value = 'mixer'
  }
})

const dbMarks = [6, 0, -6, -12, -18, -24, -30, -36, -42, -48, -54, -60]
</script>

<style scoped>
.mixer-wrapper {
  position: relative;
  height: 100%;
  background: #1a1a2e;
  color: #eee;
  overflow: hidden;
}

/* Fade suave */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.5s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

/* Loading Screen */
.loading-screen {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #1a1a2e;
}

.loading-content {
  width: 340px;
  text-align: center;
}

.project-title {
  font-size: 28px;
  font-weight: 600;
  letter-spacing: 2px;
  margin-bottom: 12px;
}

.loading-stage {
  font-size: 13px;
  opacity: 0.6;
  margin-bottom: 24px;
}

.progress-track {
  height: 4px;
  background: #0d0d1a;
  border-radius: 4px;
  overflow: hidden;
}

.progress-bar {
  height: 100%;
  background: linear-gradient(90deg, #00ff88, #00ccff);
  transition: width 0.2s ease;
}

.progress-text {
  margin-top: 10px;
  font-size: 12px;
  opacity: 0.7;
}

/* Mixer Screen */
.mixer-screen {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: flex-start;
  overflow-x: auto;
  gap: 6px;
  padding: 20px;
  background: #2c2c2c;
}

.channel {
  width: 100px;
  background: #3a3a3a;
  padding: 10px;
  display: flex;
  flex-direction: column;
  align-items: center;
  border-radius: 6px;
}

.db-display {
  background: #1f1f1f;
  padding: 4px 10px;
  border-radius: 14px;
  font-size: 14px;
  color: #ddd;
  margin-bottom: 10px;
}

.strip {
  display: flex;
  height: 240px;
  align-items: stretch;
  position: relative;
}

.scale {
  font-size: 9px;
  color: #aaa;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  margin-right: 4px;
}

.fader {
  height: 100%;
  width: 20px;
  margin-right: 6px;
}

.meter {
  width: 14px;
  background: #111;
  position: relative;
  border-radius: 2px;
}

.meter-bar {
  position: absolute;
  bottom: 0;
  width: 100%;
  background: linear-gradient(to top, #00ff66, #ffff00, #ff3300);
}

.channel-name {
  width: 90px;
  margin-top: 10px;
}

.buttons {
  margin-top: 8px;
  display: flex;
  gap: 8px;
}

.btn {
  min-width: 30px;
  min-height: 30px;
  background: #1e1e1e;
  color: #ddd;
  font-weight: bold;
  text-align: center;
  align-items: center;
  justify-content: center;
  border-radius: 3px;
  padding: 4px;
  cursor: pointer;
}

.btn.active {
  background: #f4a742;
  color: black;
}
</style>