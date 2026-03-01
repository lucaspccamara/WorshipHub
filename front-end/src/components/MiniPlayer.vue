<template>
  <div class="mini-player">

    <q-btn
      flat
      :icon="isPlaying ? 'fa fa-pause' : 'fa fa-play'"
      @click="isPlaying ? stop() : play()"
    />

    <q-slider
      class="col q-mx-md"
      :min="0"
      :max="duration"
      v-model="currentTime"
      @update:model-value="seek"
    />

    <div class="time">
      {{ formatTime(currentTime) }} /
      {{ formatTime(duration) }}
    </div>

  </div>
</template>

<script setup>
import { useAudioMixer } from '../composables/useAudioMixer'

const {
  play,
  stop,
  seek,
  isPlaying,
  duration,
  currentTime
} = useAudioMixer()

function formatTime(sec) {
  if (!sec) return '0:00'
  const m = Math.floor(sec / 60)
  const s = Math.floor(sec % 60)
  return `${m}:${s < 10 ? '0' : ''}${s}`
}
</script>

<style scoped>
.mini-player {
  display: flex;
  align-items: center;
  padding: 10px;
  background: #111;
  color: white;
}
.time {
  font-size: 12px;
}
</style>