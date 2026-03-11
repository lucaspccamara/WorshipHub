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
      color="primary"
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
  padding: 10px 20px;
  background: #111;
  color: white;
  border-top: 1px solid #333;
}

.time {
  font-size: 13px;
  font-family: 'Roboto Mono', monospace;
  min-width: 90px;
  text-align: right;
  opacity: 0.8;
}
</style>