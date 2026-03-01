<template>
  <div class="mixer">
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

      <!-- PAN -->
      <!-- <q-knob
        size="60px"
        :min="-100"
        :max="100"
        v-model="track.pan"
        @update:model-value="val => setPan(track, val)"
        color="grey-5"
      /> -->

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
</template>

<script setup>
import { onMounted } from 'vue'
import { useAudioMixer } from '../composables/useAudioMixer'
import MeterCanvas from './MeterCanvas.vue'

const {
  tracks,
  loadMockTracks,
  setDb,
  toggleMute,
  toggleSolo
} = useAudioMixer()

const dbMarks = [6, 0, -6, -12, -18, -24, -30, -36, -42, -48, -54, -60]

onMounted(() => {
  loadMockTracks()
})
</script>

<style scoped>
.mixer {
  max-width: 97vw;
  display: flex;
  justify-self: center;
  gap: 6px;
  padding: 20px;
  background: #2c2c2c;
  overflow-x: auto;
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