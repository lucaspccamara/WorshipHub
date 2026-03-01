<template>
  <div class="q-pa-md">
    <q-date
      v-model="date"
      :events="events"
      event-color="orange"
      style="width: 100%;"
      class="q-mb-md"
    />

    <q-tab-panels v-model="date" animated transition-prev="fade" transition-next="fade" style="background: none;">
      <q-tab-panel class="q-pa-none" :name="date">
        <template v-if="currentPanel">
          <!-- ESCALA -->
          <q-card class="row bg-grey-5">
            <q-item v-for="position in currentPanel.positions" :key="position.positionId"
              :class="position.highlight ? 'role-highlight col-xs-6 col-md-3 q-pa-md' : 'col-xs-6 col-md-3 q-pa-md'"
            >
              <q-item-section avatar>
                <q-avatar color="primary" icon="fa-solid fa-user" />
              </q-item-section>
              <q-item-section>
                <q-item-label>{{ PositionOptions.find(p => p.value == position.positionId).label }}</q-item-label>
                <q-item-label caption>{{ position.member }}</q-item-label>
              </q-item-section>
            </q-item>
          </q-card>

          <!-- MÚSICAS -->
          <div class="text-h6 q-mt-md">Músicas</div>
          <q-card class="card-container">
            <q-list>
              <q-item
                clickable
                v-for="music in currentPanel.musics"
                :key="music.title"
                class="q-pa-md q-mt-sm bg-grey-5 card-music"
                @click="openMusicOverview(music)"
              >
                <q-img class="music-bg" :src="music.imageUrl" fit="cover" />
                <div class="overlay"></div>
                <q-item-section class="music-content">
                  <q-item-label class="music-title">{{ music.title }}</q-item-label>
                  <q-item-label class="music-artist">{{ music.artist }}</q-item-label>
                  <q-item-label class="music-details">{{ music.details }}</q-item-label>
                </q-item-section>
              </q-item>
            </q-list>
          </q-card>
        </template>
        <template v-else>
          <div class="no-events-container">
            <div class="ghost-container">
              <q-icon name="fa-solid fa-ghost" class="ghost" size="50px" color="grey-7" />
            </div>
            <div class="text-center q-pa-md text-grey-7">
              Nenhum evento programado para esta data
            </div>
          </div>
        </template>
      </q-tab-panel>
    </q-tab-panels>
  </div>

  <q-dialog
    v-model="dialogMusic"
    maximized
    transition-show="slide-up"
    transition-hide="slide-down"
  >
    <MusicOverview :music="selectedMusic" />
  </q-dialog>
</template>

<script setup>
import { computed, ref, onMounted } from 'vue';
import { date as QuasarDate } from 'quasar';
import api from '../api';
import { PositionOptions } from '../constants/PositionOptions';
import MusicOverview from './MusicOverview.vue'

const filter = {};
const date = ref('');
let startDate = ref('');
let endDate = ref('');
const dialogMusic = ref(false)
const selectedMusic = ref(null)

const panels = ref([]);

const events = computed(() => panels.value.map((panel) => panel.date));

const currentPanel = computed(() => panels.value.find((panel) => panel.date === date.value) || null);

function setDefaultDates() {
  const today = new Date();
  const firstDay = new Date(today.getFullYear(), today.getMonth(), 1).toISOString().split('T')[0];
  const lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0).toISOString().split('T')[0];

  startDate.value = firstDay;
  endDate.value = lastDay;
};

function selectNextDate() {
  const today = new Date();
  const dayOfWeek = today.getDay(); // 0 = Domingo, 1 = Segunda, ..., 6 = Sábado

  if (dayOfWeek === 0) {
    // Hoje é domingo, usa a data de hoje
    date.value = QuasarDate.formatDate(today, 'YYYY/MM/DD');
  } else {
    // Calcula o próximo domingo
    const nextSunday = new Date();
    nextSunday.setDate(today.getDate() + (7 - dayOfWeek));
    date.value = QuasarDate.formatDate(nextSunday, 'YYYY/MM/DD');
  }
}

function getCalendar() {
  panels.value = []
  filter.value = {
    startDate: startDate.value,
    endDate: endDate.value,
  }

  api.getPost('homes/calendar', filter.value).then((response) => {
    panels.value.splice(0, panels.value.length, ...response.data);
  }).finally(() => {
    selectNextDate();
  });
};

function openMusicOverview(music) {
  selectedMusic.value = music
  dialogMusic.value = true
}

onMounted(() => {
  setDefaultDates();
  getCalendar();
});
</script>

<style lang="scss">
.no-events-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 20px;
  opacity: 0.8;
  gap: 10px;
}

.ghost-container {
  display: inline-block;
  animation: ghostMoveX 4s infinite ease-in-out alternate;
}

.ghost {
  animation: ghostMoveY 2s infinite ease-in-out;
}

@keyframes ghostMoveX {
  0% { transform: translateX(-30px); }
  100% { transform: translateX(30px); }
}

@keyframes ghostMoveY {
  0%, 100% { transform: translateY(0px); }
  50% { transform: translateY(-5px); }
}

.role-highlight {
  .q-avatar {
    outline: solid orange 3px;
  }

  .q-item__label {
    font-weight: bold;
  }
}

.card-container {
  border-radius: 8px;
  background: none;
}

.card-music {
  position: relative;
  display: flex;
  align-items: center;
  padding: 20px;
  border-radius: 8px;
  color: white;
  overflow: hidden;
  min-height: 120px;

  .music-bg {
    position: absolute;
    inset: 0;
    filter: brightness(0.8);
    
    /* Aplicando fade diretamente na imagem */
    mask-image: linear-gradient(to right, rgba(0, 0, 0, 1) 0%, rgba(0, 0, 0, 0.7) 20%, rgba(0, 0, 0, 0.3) 50%, rgba(0, 0, 0, 0.05) 80%);
    -webkit-mask-image: linear-gradient(to right, rgba(0, 0, 0, 1) 0%, rgba(0, 0, 0, 0.7) 20%, rgba(0, 0, 0, 0.3) 50%, rgba(0, 0, 0, 0.05) 80%);
  }

  /* Overlay para efeito de fade */
  .overlay {
    position: absolute;
    inset: 0;
    background: linear-gradient(to right, rgba(0, 0, 0, 0.8) 0%, rgba(0, 0, 0, 0.3) 40%, rgba(0, 0, 0, 0.1) 80%);
    z-index: 1;
  }

  .music-content {
    position: relative;
    z-index: 1;
  }

  .music-title {
    font-size: 18px;
    font-weight: bold;
  }

  .music-artist {
    font-size: 12px;
    font-style: italic;
  }

  .music-details {
    font-size: 14px;
  }
}
</style>