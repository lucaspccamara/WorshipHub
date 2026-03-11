<template>
  <q-card class="column">
    <q-bar class="card-header">
      <q-space />
      <q-btn dense flat icon="fa fa-close" v-close-popup>
        <q-tooltip>Fechar</q-tooltip>
      </q-btn>
    </q-bar>

    <q-tabs v-model="tab" dense>
      <q-tab name="details" label="Detalhes" />
      <q-tab name="mixer" label="Mixer VS" />
      <q-tab name="chords" label="Cifra" />
    </q-tabs>

    <q-separator />

    <q-tab-panels v-model="tab" animated class="col">

      <q-tab-panel name="details">
        <!-- <MusicDetails /> -->
      </q-tab-panel>

      <q-tab-panel name="mixer" class="q-pa-none">
        <MixerVS />
      </q-tab-panel>

      <q-tab-panel name="chords">
        <!-- <ChordSheet /> -->
      </q-tab-panel>

    </q-tab-panels>

    <transition name="cinematic-reveal">
      <MiniPlayer v-if="!isLoading" />
    </transition>

  </q-card>
</template>

<script setup>
import { ref } from 'vue';
//import MusicDetails from './MusicDetails.vue'
import MixerVS from './MixerVS.vue'
//import ChordSheet from './ChordSheet.vue'
import MiniPlayer from './MiniPlayer.vue'
import { useAudioMixer } from '../composables/useAudioMixer'

const { isLoading } = useAudioMixer()
const tab = ref('mixer')
</script>

<style scoped>
.cinematic-reveal-enter-active {
  transition: all 0.8s cubic-bezier(0.16, 1, 0.3, 1);
}

.cinematic-reveal-enter-from {
  opacity: 0;
  transform: translateY(20px);
}

.cinematic-reveal-enter-to {
  opacity: 1;
  transform: translateY(0);
}
</style>