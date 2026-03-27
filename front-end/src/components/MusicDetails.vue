<template>
  <div class="q-pa-md row justify-center overflow-hidden">
    <div v-if="loading" class="col-12 flex flex-center q-pa-xl">
      <q-spinner-dots size="40px" color="primary" />
    </div>

    <div v-else-if="music" class="col-12 col-md-10 col-lg-8">
      <!-- CABEÇALHO / CAPA E METADADOS -->
      <div class="row q-mb-md items-center no-wrap-sm">
        
        <!-- Album Art / Vinyl Scene -->
        <div class="col-12 col-sm-5 col-md-5 flex flex-center animate-slide-up stagger-1">
          <div class="album-scene" :class="{ 'is-playing': isPlaying }" @click="isPlaying = !isPlaying">
            <div class="vinyl-slider">
              <div class="vinyl-spinner">
                <q-img :src="music.imageUrl || '/default-music-cover.png'" class="vinyl-label" :ratio="1" />
                <div class="vinyl-hole"></div>
              </div>
            </div>
            <q-img 
              :src="music.imageUrl || '/default-music-cover.png'" 
              class="album-cover" 
              :ratio="1"
            />
          </div>
        </div>

        <div class="col-12 col-sm-7 col-md-7 column justify-center animate-slide-up stagger-2">
          <div class="text-h4 text-weight-bold q-mb-xs title-text">{{ music.title }}</div>
          <div class="text-h6 text-grey-8 q-mb-md wrap-text" :title="music.artist">{{ music.artist || 'Artista Desconhecido' }}</div>
          
          <q-chip v-if="music.album" class="q-ml-none q-mb-md album-chip" color="primary" text-color="white" icon="fa-solid fa-compact-disc">
            {{ music.album }}
          </q-chip>

          <!-- GRADES DE CARACTERÍSTICAS -->
          <div class="row q-col-gutter-md q-mt-sm">
            <div class="col-6 col-sm-3">
              <q-card flat bordered class="text-center q-pa-sm bg-grey-1 detail-card">
                <q-icon name="fa-solid fa-music" size="sm" color="primary" class="icon-bounce q-mb-xs" />
                <div class="text-caption text-weight-bold text-uppercase text-grey-7">Tom</div>
                <div class="text-subtitle1 text-weight-bold">{{ music.noteBase ? `${music.noteBase}${music.noteMode === 'minor' ? 'm' : ''}` : '--' }}</div>
              </q-card>
            </div>
            <div class="col-6 col-sm-3">
              <q-card flat bordered class="text-center q-pa-sm bg-grey-1 detail-card">
                <q-icon name="fa-solid fa-heart-pulse" size="sm" color="orange" class="icon-pulse q-mb-xs" />
                <div class="text-caption text-weight-bold text-uppercase text-grey-7">BPM</div>
                <div class="text-subtitle1 text-weight-bold">{{ music.bpm ? Math.round(music.bpm) : '--' }}</div>
              </q-card>
            </div>
            <div class="col-6 col-sm-3">
              <q-card flat bordered class="text-center q-pa-sm bg-grey-1 detail-card">
                <q-icon name="fa-solid fa-ruler" size="sm" color="secondary" class="icon-tilt q-mb-xs" />
                <div class="text-caption text-weight-bold text-uppercase text-grey-7">Tempo</div>
                <div class="text-subtitle1 text-weight-bold">{{ music.timeSignature || '--' }}</div>
              </q-card>
            </div>
            <div class="col-6 col-sm-3">
              <q-card flat bordered class="text-center q-pa-sm bg-grey-1 detail-card">
                <q-icon name="fa-solid fa-clock" size="sm" color="accent" class="icon-spin q-mb-xs" />
                <div class="text-caption text-weight-bold text-uppercase text-grey-7">Duração</div>
                <div class="text-subtitle1 text-weight-bold">{{ music.duration || formatDuration(music.durationSeconds) || '--' }}</div>
              </q-card>
            </div>
          </div>
        </div>
      </div>

      <q-separator class="q-my-xl opacity-30 animate-fade-in stagger-3" />

      <!-- PLAYER DE VÍDEO / REFERÊNCIA -->
      <div v-if="embedUrl" class="q-mt-lg animate-slide-up stagger-4">
        <div class="text-h6 q-mb-md">
          <q-icon name="fa-brands fa-youtube" color="red" class="q-mr-sm" size="sm" /> 
          Clipe Oficial / Referência
        </div>
        <q-video
          :src="embedUrl"
          :ratio="16/9"
          class="rounded-borders shadow-8 video-player"
        />
      </div>
      
      <div v-else class="text-center q-pa-xl text-grey-6 border-dotted q-mt-lg rounded-borders animate-slide-up stagger-4">
        <q-icon name="fa-solid fa-video-slash" size="xl" class="q-mb-md opacity-50" />
        <div class="text-subtitle1">Nenhum vídeo de referência cadastrado para esta música.</div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, watch, onMounted, computed } from 'vue';
import { Notify } from 'quasar';
import api from '../api';

const props = defineProps({
  musicId: {
    type: Number,
    required: true
  }
});

const music = ref(null);
const loading = ref(false);
const isPlaying = ref(false);

const embedUrl = computed(() => {
  if (!music.value || !music.value.videoUrl) return null;
  const url = music.value.videoUrl;

  // Transform YouTube normal or shorten URL to embed URL
  const regExp = /^.*(youtu.be\/|v\/|u\/\w\/|embed\/|watch\?v=|&v=)([^#&?]*).*/;
  const match = url.match(regExp);

  if (match && match[2].length === 11) {
    return `https://www.youtube.com/embed/${match[2]}`;
  }
  
  return null;
});

const formatDuration = (seconds) => {
  if (!seconds) return null;
  const m = Math.floor(seconds / 60);
  const s = seconds % 60;
  return `${m}:${s.toString().padStart(2, '0')}`;
};

const fetchDetails = async () => {
  if (!props.musicId) return;
  loading.value = true;
  try {
    const response = await api.get(`musics/${props.musicId}`);
    music.value = response.data;
    
    setTimeout(() => {
      isPlaying.value = true;
    }, 1000);
  } catch (err) {
    console.error(err);
    Notify.create({ type: 'negative', message: 'Erro ao buscar os detalhes da música.' });
  } finally {
    loading.value = false;
  }
};

watch(() => props.musicId, (newId) => {
  if (newId) fetchDetails();
});

onMounted(() => {
  fetchDetails();
});
</script>

<style scoped>
/* Album Scene and Vinyl Record effect */
.album-scene {
  position: relative;
  width: 200px;
  height: 200px;
  perspective: 1000px;
  cursor: pointer;
  margin: 20px 40px 20px 0; /* space for the vinyl to pop out on right */
}

@media (min-width: 600px) {
  .album-scene {
    width: 240px;
    height: 240px;
    margin: 20px 60px 20px 0;
  }
}

.vinyl-slider {
  position: absolute;
  top: 2%;
  left: 2%;
  width: 96%;
  height: 96%;
  z-index: 1;
  transition: transform 0.6s cubic-bezier(0.34, 1.56, 0.64, 1);
}

.album-scene.is-playing .vinyl-slider {
  transform: translateX(45%); /* Slides out half-way */
}

.vinyl-spinner {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  background: #111;
  box-shadow: inset 0 0 0 4px #222, 0 4px 15px rgba(0,0,0,0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  /* Complex grooves for realistic vinyl texture */
  background-image: 
    conic-gradient(from 0deg, rgba(255,255,255,0.05) 0%, transparent 10%, transparent 40%, rgba(255,255,255,0.05) 50%, transparent 60%, transparent 90%, rgba(255,255,255,0.05) 100%),
    radial-gradient(transparent 30%, rgba(255,255,255,0.03) 32%, transparent 34%),
    radial-gradient(transparent 45%, rgba(255,255,255,0.04) 47%, transparent 49%),
    radial-gradient(transparent 60%, rgba(255,255,255,0.03) 62%, transparent 64%),
    radial-gradient(transparent 75%, rgba(255,255,255,0.05) 77%, transparent 79%);
}

.album-scene.is-playing .vinyl-spinner {
  animation: spin 3s linear infinite;
  animation-play-state: running;
}

.vinyl-label {
  width: 33%;
  height: 33%;
  border-radius: 50%;
  pointer-events: none;
  box-shadow: 0 0 0 2px #000;
}

.vinyl-hole {
  position: absolute;
  width: 8%;
  height: 8%;
  background: white;
  border-radius: 50%;
  border: 1px solid #111;
  box-shadow: inset 0 2px 5px rgba(0,0,0,0.5);
  z-index: 10;
}

.album-cover {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  border-radius: 6px;
  z-index: 2;
  box-shadow: -5px 0 25px rgba(0,0,0,0.5);
  transition: transform 0.5s ease-out;
  transform-origin: left center;
}

.album-scene.is-playing .album-cover {
  transform: rotateY(-18deg) scale(0.98);
  box-shadow: inset -1px 0 8px rgba(255,255,255,0.2), 15px 0 30px rgba(0,0,0,0.6);
}

@keyframes spin {
  100% { transform: rotate(360deg); }
}

/* Detail Cards */
.detail-card {
  transition: all 0.3s ease;
  border: 1px solid rgba(0, 0, 0, 0.05);
  height: 100%;
}
.detail-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 10px 20px rgba(0,0,0,0.08);
  border-color: var(--q-primary);
}

/* Icons styling and hover animations */
.icon-bounce, .icon-pulse, .icon-tilt, .icon-spin {
  transition: transform 0.3s ease;
}
.detail-card:hover .icon-bounce { transform: translateY(-3px); }
.detail-card:hover .icon-pulse { animation: scalePulse 0.8s infinite alternate; }
.detail-card:hover .icon-tilt { transform: rotate(15deg); }
.detail-card:hover .icon-spin { transform: rotate(180deg); }

@keyframes scalePulse {
  0% { transform: scale(1); }
  100% { transform: scale(1.2); }
}

.video-player {
  transition: transform 0.3s ease;
}
.video-player:hover {
  transform: scale(1.01);
}

/* Title, Artist and Album Wrap handling */
.title-text, .wrap-text {
  letter-spacing: -0.5px;
  word-wrap: break-word;
  overflow-wrap: break-word;
  padding-right: 16px; /* Folga de segurança */
  white-space: normal; /* Garante a quebra de linha */
  max-width: 100%;
}

.album-chip {
  max-width: 100%;
  overflow: hidden; /* Importante para o ellipsis no child */
}

:deep(.album-chip .q-chip__content) {
  display: block !important;
  white-space: nowrap !important;
  overflow: hidden !important;
  text-overflow: ellipsis !important;
  width: 100%;
}

@media (max-width: 600px) {
  .title-text {
    font-size: 1.5rem; /* Reduz um pouco no mobile para caber mais nome */
    line-height: 2rem;
  }
}

/* Base animations */
.animate-slide-up {
  opacity: 0;
  animation: slideUp 0.6s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

.animate-fade-in {
  opacity: 0;
  animation: fadeIn 0.8s ease forwards;
}

@keyframes slideUp {
  0% { opacity: 0; transform: translateY(30px); }
  100% { opacity: 1; transform: translateY(0); }
}
@keyframes fadeIn {
  0% { opacity: 0; }
  100% { opacity: 1; }
}

/* Staggers */
.stagger-1 { animation-delay: 0.1s; }
.stagger-2 { animation-delay: 0.2s; }
.stagger-3 { animation-delay: 0.3s; }
.stagger-4 { animation-delay: 0.4s; }

/* Utilities */
.border-dotted {
  border: 2px dashed #ddd;
}
.opacity-50 {
  opacity: 0.5;
}
.opacity-30 {
  opacity: 0.3;
}
</style>
