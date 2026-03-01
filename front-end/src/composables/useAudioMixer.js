import { ref } from 'vue'

const AudioCtx = window.AudioContext || window.webkitAudioContext
const audioContext = new AudioCtx()

const tracks = ref([])
const isPlaying = ref(false)
const duration = ref(0)
const currentTime = ref(0)

let startTime = 0

function dbToGain(db) {
  if (db <= -60) return 0
  return Math.pow(10, db / 20)
}

export function useAudioMixer() {

  async function loadMockTracks() {

    if (tracks.value.length) return

    const trackFiles = [
      { name: 'Click', file: '/mock/01_Click Track.aac' },
      { name: 'Guide', file: '/mock/02_Guide.aac' },
      { name: 'Drums', file: '/mock/03_Drums.aac' },
      { name: 'Bass', file: '/mock/04_Bass.aac' },
      { name: 'AG', file: '/mock/05_AG.aac' },
      { name: 'EG 1', file: '/mock/06_EG 1.aac' },
      { name: 'EG 2', file: '/mock/07_EG 2.aac' },
      { name: 'EG 3', file: '/mock/08_EG 3.aac' },
      { name: 'EG 4', file: '/mock/09_EG 4.aac' },
      { name: 'EG 5', file: '/mock/10_EG 5.aac' },
      { name: 'EG 6', file: '/mock/11_EG 6.aac' },
      { name: 'EG 7', file: '/mock/12_EG 7.aac' },
      { name: 'Piano', file: '/mock/13_Piano.aac' },
      { name: 'Keys 1', file: '/mock/14_Keys 1.aac' },
      { name: 'Keys 2', file: '/mock/15_Keys 2.aac' },
      { name: 'Keys 3', file: '/mock/16_Keys 3.aac' },
      { name: 'Keys 4', file: '/mock/17_Keys 4.aac' },
      { name: 'Synth Bass', file: '/mock/18_Synth Bass.aac' },
      { name: 'Synth Loop', file: '/mock/19_Synth Loop.aac' }
    ]

    for (const t of trackFiles) {

      const response = await fetch(t.file)
      const buffer = await audioContext.decodeAudioData(
        await response.arrayBuffer()
      )

      duration.value = buffer.duration

      const gain = audioContext.createGain()
      const panner = audioContext.createStereoPanner()
      const analyser = audioContext.createAnalyser()

      analyser.fftSize = 2048
      analyser.smoothingTimeConstant = 0

      gain.connect(panner)
      panner.connect(analyser)
      analyser.connect(audioContext.destination)

      tracks.value.push({
        name: t.name,
        buffer,
        source: null,
        gain,
        panner,
        analyser,
        db: 0,
        pan: 0,
        mute: false,
        solo: false
      })
    }
  }

  function createSources(offset = 0) {
    tracks.value.forEach(track => {

      const source = audioContext.createBufferSource()
      source.buffer = track.buffer

      source.connect(track.gain)

      source.start(0, offset)

      track.source = source
    })
  }

  function updateAudioRouting() {
    const soloed = tracks.value.filter(t => t.solo)

    tracks.value.forEach(track => {

      let targetGain = 0

      if (soloed.length > 0) {
        targetGain = track.solo ? dbToGain(track.db) : 0
      } else {
        targetGain = track.mute ? 0 : dbToGain(track.db)
      }

      // 🔥 pequena rampa evita clicks
      track.gain.gain.cancelScheduledValues(audioContext.currentTime)
      track.gain.gain.linearRampToValueAtTime(
        targetGain,
        audioContext.currentTime + 0.02
      )
    })
  }

  function play(offset = currentTime.value) {

    if (isPlaying.value) return

    if (audioContext.state === 'suspended') {
      audioContext.resume()
    }

    startTime = audioContext.currentTime - offset

    createSources(offset)

    isPlaying.value = true

    requestAnimationFrame(updateTime)
  }

  function updateTime() {
    if (!isPlaying.value) return
    currentTime.value = audioContext.currentTime - startTime
    requestAnimationFrame(updateTime)
  }

  function stop() {

    tracks.value.forEach(track => {
      if (track.source) {
        try { track.source.stop() } catch {}
        track.source.disconnect()
        track.source = null
      }
    })

    isPlaying.value = false
  }

  function seek(time) {
    stop()
    currentTime.value = time
    play(time)
  }

  function setDb(track, db) {
    track.db = db
    updateAudioRouting()
  }

  function toggleMute(track) {
    track.mute = !track.mute
    updateAudioRouting()
  }

  function toggleSolo(track) {
    track.solo = !track.solo
    updateAudioRouting()
  }

  function setPan(track, value) {
    track.pan = value
    track.panner.pan.value = value / 100
  }

  return {
    tracks,
    isPlaying,
    duration,
    currentTime,
    loadMockTracks,
    play,
    stop,
    seek,
    setDb,
    toggleMute,
    toggleSolo,
    setPan
  }
}