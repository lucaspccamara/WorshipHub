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
        <q-tab-panel v-for="(panel, index) in panels" :key="index" :name="panel.date">
          <q-card class="row q-pa-md bg-grey-5">
            <div v-for="role in panel.roles" :key="role.name">
              <q-item>
                <q-item-section avatar>
                  <q-avatar color="primary" icon="fa fa-user" />
                </q-item-section>
                <q-item-section>
                  <q-item-label>{{ role.name }}</q-item-label>
                  <q-item-label caption>{{ role.person }}</q-item-label>
                </q-item-section>
              </q-item>
            </div>
          </q-card>
          <div class="text-h6 q-mt-md">Músicas</div>
          <q-card style="height: 350px;">
            <q-list class="scroll">
              <q-item clickable v-for="song in panel.songs" :key="song.title" class="q-pa-md bg-blue-3 card-music">
                <q-item-section>
                  <q-img src="https://via.placeholder.com/150" alt="Album cover" style="width: 100px; height: 100px;" />
                </q-item-section>
                <q-item-section>
                  <q-item-label>{{ song.title }}</q-item-label>
                  <q-item-label caption>{{ song.details }}</q-item-label>
                </q-item-section>
              </q-item>
            </q-list>
          </q-card>
        </q-tab-panel>
      </q-tab-panels>
    </template>
  </q-splitter>
</template>

<script setup>
import { ref } from 'vue';
import { Notify } from 'quasar';

const date = ref('2024/07/07');
const events = ref(['2024/07/07', '2024/07/14', '2024/07/21']);
const splitterModel = ref(50);

const panels = ref([
  {
    date: '2024/07/07',
    roles: [
      { name: 'Ministro', person: 'Fulano' },
      { name: 'Baixista', person: 'Ciclano' },
      { name: 'Baterista', person: 'Beltrano' },
      { name: 'Guitarrista', person: 'Sicrano' },
      { name: 'Vocalista', person: 'Fulana' },
    ],
    songs: [
      { title: 'Tu És + Águas Purificadoras', details: 'Tom: D BPM: 71 Tempo: 4/4 Duração: 8:03' },
      { title: 'A Boa Parte', details: 'Tom: D BPM: 71 Tempo: 4/4 Duração: 8:03' },
      { title: 'Tu És + Águas Purificadoras', details: 'Tom: D BPM: 71 Tempo: 4/4 Duração: 8:03' },
      { title: 'A Boa Parte', details: 'Tom: D BPM: 71 Tempo: 4/4 Duração: 8:03' },
      { title: 'Tu És + Águas Purificadoras', details: 'Tom: D BPM: 71 Tempo: 4/4 Duração: 8:03' },
    ],
  },
]);

const showSchedule = (date) => {
  Notify.create({
    message: `Você está escalado no dia`,
    color: 'info'
  });
};
</script>

<style lang="scss">
.main-card{
  height: 100vh !important;
}

.card-music {
  flex: 1 1 calc(100% - 1rem);
  display: flex;
  flex-direction: row;
  align-items: center;

  .q-item {
    padding: 0;

    .q-item-section {
      display: flex;
      flex-direction: column;
      align-items: center;
    }
  }
}
</style>