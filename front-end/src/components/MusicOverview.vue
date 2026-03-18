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
      <q-tab name="mixer" label="Mixer" />
      <q-tab name="chords" label="Cifra" />
    </q-tabs>

    <q-separator />

    <!-- Keep-alive preserva o estado do Mixer e as camadas de GPU ao trocar de aba -->
    <q-tab-panels 
      v-model="tab" 
      :animated="!isMobile" 
      keep-alive
      class="col"
    >

      <q-tab-panel name="details">
        <!-- <MusicDetails /> -->
      </q-tab-panel>

      <q-tab-panel name="mixer" class="q-pa-none">
        <MixerVS :musicId="musicId" :ready="showContent" />
      </q-tab-panel>

      <q-tab-panel name="chords">
        <!-- <ChordSheet /> -->
      </q-tab-panel>

    </q-tab-panels>

    <transition name="fade-slow">
      <MiniPlayer v-if="showContent && tracks.length > 0" />
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

const props = defineProps({
  musicId: {
    type: Number,
    required: true
  }
})

const { isLoading, tracks } = useAudioMixer()
const tab = ref('mixer')
const isMobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)

// Estado unificado para revelação (Mixer + MiniPlayer)
const showContent = ref(false)

import { watch } from 'vue'
watch(isLoading, (loading, oldLoading) => {
  if (oldLoading === true && loading === false) {
    // Sincroniza a revelação de ambos após o Splash
    setTimeout(() => {
      showContent.value = true
    }, 400)
  }
}, { immediate: !isLoading.value && tracks.value.length > 0 })

// Se já terminou de carregar antes de montar (ex: cache)
if (!isLoading.value && tracks.value.length > 0) {
  showContent.value = true
}
</script>

<style scoped>
/* Transição lenta e elegante */
.fade-slow-enter-active,
.fade-slow-leave-active {
  transition: opacity 0.8s ease-in-out;
}
.fade-slow-enter-from,
.fade-slow-leave-to {
  opacity: 0;
}
</style>