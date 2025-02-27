<template>
  <q-splitter
    v-model="splitterModel"
    horizontal
  >

    <template v-slot:before>
      <div class="q-pa-md">
        <q-date
          v-model="date"
          :events="events"
          event-color="orange"
          style="width: 100%;"
        />
      </div>
    </template>

    <template v-slot:after>
      <q-tab-panels v-model="date" animated transition-prev="jump-up" transition-next="jump-up">
        <q-tab-panel :name="date">
          <template v-if="currentPanel">
            <!-- ESCALA -->
            <q-card class="row q-pa-md bg-grey-5">
              <q-item v-for="role in currentPanel.roles" :key="role.name" :class="role.highlight ? 'role-highlight col-xs-6 col-md-4' : 'col-xs-6 col-md-4'">
                <q-item-section avatar>
                  <q-avatar color="primary" icon="fa-solid fa-user" />
                </q-item-section>
                <q-item-section>
                  <q-item-label>{{ role.name }}</q-item-label>
                  <q-item-label caption>{{ role.person }}</q-item-label>
                </q-item-section>
              </q-item>
            </q-card>
  
            <!-- MÚSICAS -->
            <div class="text-h6 q-mt-md">Músicas</div>
            <q-card>
              <q-list>
                <q-item
                  clickable
                  v-for="song in currentPanel.songs"
                  :key="song.title"
                  class="q-pa-md q-mt-sm bg-grey-5 card-music"
                >
                  <q-img class="music-bg" :src="song.image" fit="cover" />
                  <div class="overlay"></div>
                  <q-item-section class="music-content">
                    <q-item-label class="music-title">{{ song.title }}</q-item-label>
                    <q-item-label class="music-author">{{ song.author }}</q-item-label>
                    <q-item-label class="music-details">{{ song.details }}</q-item-label>
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
    </template>
  </q-splitter>
</template>

<script setup>
import { computed, ref, onMounted } from 'vue';
import { date as QuasarDate } from 'quasar';

const date = ref('');
const splitterModel = ref(50);

const panels = ref([
  {
    date: '2025/03/02',
    roles: [
      { name: 'Ministro', person: 'Fulano', highlight: true },
      { name: 'Baixista', person: 'Ciclano', highlight: false },
      { name: 'Baterista', person: 'Beltrano', highlight: false },
      { name: 'Guitarrista', person: 'Sicrano', highlight: false },
      { name: 'Vocalista', person: 'Fulana', highlight: false },
    ],
    songs: [
      {
        id: 0,
        title: 'Tu És + Águas Purificadoras',
        author: 'FHOP',
        details: 'Tom: D BPM: 71 Tempo: 4/4 Duração: 8:03',
        image: 'https://mtracks.azureedge.net/public/images/albums/284/8772.jpg'
      },
      {
        id: 1,
        title: 'Lá',
        author: 'Paulo César Baruk',
        details: 'Tom: C BPM: 100 Tempo: 4/4 Duração: 4:07',
        image: 'https://mtracks.azureedge.net/public/images/albums/284/3328.jpg'
      },
      {
        id: 2,
        title: 'Estamos de Pé',
        author: 'Marcus Salles',
        details: 'Tom: A BPM: 104 Tempo: 4/4 Duração: 6:32',
        image: 'https://mtracks.azureedge.net/public/images/albums/284/8878.jpg'
      },
      {
        id: 3,
        title: 'Tudo é Teu / Nova Criatura / Rede ao Mar',
        author: 'Morada',
        details: 'Tom: B BPM: 158 Tempo: 4/4 Duração: 5:09',
        image: 'https://mtracks.azureedge.net/public/images/albums/284/7400.jpg'
      },
      { 
        id: 4,
        title: 'Bondade de Deus',
        author: 'Isaias Saad',
        details: 'Tom: G BPM: 68 Tempo: 4/4 Duração: 5:45',
        image: 'https://mtracks.azureedge.net/public/images/albums/284/8201.jpg'
      },
    ],
  },
  {
    date: '2025/03/09',
    roles: [
      { name: 'Ministro', person: 'Fulano', highlight: false },
      { name: 'Baixista', person: 'Ciclano', highlight: true },
      { name: 'Baterista', person: 'Beltrano', highlight: false },
      { name: 'Guitarrista', person: 'Sicrano', highlight: false },
      { name: 'Vocalista', person: 'Fulana', highlight: false },
    ],
    songs: [
      {
        id: 5,
        title: 'Atos 2',
        author: 'Gabriel Guedes',
        details: 'Tom: C BPM: 72 Tempo: 6/8 Duração: 7:39',
        image: 'https://mtracks.azureedge.net/public/images/albums/284/2418.jpg'
      },
      {
        id: 6,
        title: 'Galileu',
        author: 'Fernandinho',
        details: 'Tom: Db BPM: 116 Tempo: 4/4 Duração: 7:20',
        image: 'https://mtracks.azureedge.net/public/images/albums/284/2001.jpg'
      },
      {
        id: 7,
        title: 'Eu Também (100 Bilhões X)',
        author: 'Hillsong United',
        details: 'Tom: A BPM: 64 Tempo: 4/4 Duração: 6:58',
        image: 'https://mtracks.azureedge.net/public/images/albums/284/1171.jpg'
      },
      {
        id: 8,
        title: 'Teu Amor Não Falha',
        author: 'Nivea Soares',
        details: 'Tom: C BPM: 114 Tempo: 4/4 Duração: 6:02',
        image: 'https://mtracks.azureedge.net/public/images/albums/284/507.jpg'
      },
      {
        id: 9,
        title: 'Escape',
        author: 'Renascer Praise',
        details: 'Tom: D BPM: 64 Tempo: 4/4 Duração: 5:58',
        image: 'https://mtracks.azureedge.net/public/images/albums/284/9737.jpg'
      },
    ],
  },
]);

const events = computed(() => panels.value.map((panel) => panel.date));

const currentPanel = computed(() => panels.value.find((panel) => panel.date === date.value) || null);

onMounted(() => {
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

  .music-author {
    font-size: 12px;
    font-style: italic;
  }

  .music-details {
    font-size: 14px;
  }
}
</style>