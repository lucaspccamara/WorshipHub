import { ref } from 'vue'

const isMobile = /Android|iPhone|iPad|iPod/i.test(navigator.userAgent)

const AudioCtx = window.AudioContext || window.webkitAudioContext
const audioContext = new AudioCtx({
  sampleRate: isMobile ? 32000 : 44100
})

const tracks = ref([])
const isPlaying = ref(false)
const duration = ref(0)
const currentTime = ref(0)

const loadingStage = ref('Iniciando Engine de Áudio...')
const isLoading = ref(false)
const loaded = ref(0)
const totalTracks = ref(0)
const loadProgress = ref(0)

let startTime = 0

function dbToGain(db) {
  if (db <= -60) return 0
  return Math.pow(10, db / 20)
}

export function useAudioMixer() {

  async function loadMockTracks() {

    if (tracks.value.length) return

    isLoading.value = true
    loadProgress.value = 0

    const trackFiles = [
      { name: 'Click', url: '/mock/01_Click Track.aac' },
      { name: 'Guide', url: '/mock/02_Guide.aac' },
      { name: 'Drums', url: '/mock/03_Drums.aac' },
      { name: 'Bass', url: '/mock/04_Bass.aac' },
      { name: 'AG', url: '/mock/05_AG.aac' },
      { name: 'EG 1', url: '/mock/06_EG 1.aac' },
      { name: 'EG 2', url: '/mock/07_EG 2.aac' },
      { name: 'EG 3', url: '/mock/08_EG 3.aac' },
      { name: 'EG 4', url: '/mock/09_EG 4.aac' },
      { name: 'EG 5', url: '/mock/10_EG 5.aac' },
      { name: 'EG 6', url: '/mock/11_EG 6.aac' },
      { name: 'EG 7', url: '/mock/12_EG 7.aac' },
      { name: 'Piano', url: '/mock/13_Piano.aac' },
      { name: 'Keys 1', url: '/mock/14_Keys 1.aac' },
      { name: 'Keys 2', url: '/mock/15_Keys 2.aac' },
      { name: 'Keys 3', url: '/mock/16_Keys 3.aac' },
      { name: 'Keys 4', url: '/mock/17_Keys 4.aac' },
      { name: 'Synth Bass', url: '/mock/18_Synth Bass.aac' },
      { name: 'Synth Loop', url: '/mock/19_Synth Loop.aac' }
    ]

    loadingStage.value = 'Obtendo arquivos de áudio...'
    totalTracks.value = trackFiles.length
    loaded.value = 0

    const tasks = trackFiles.map((file, index) => {
      return async () => {
        loadingStage.value = 'Decodificando buffers de áudio...'

        const response = await fetch(file.url)
        const arrayBuffer = await response.arrayBuffer()
        const audioBuffer = await audioContext.decodeAudioData(arrayBuffer)

        loaded.value++
        loadProgress.value = Math.round((loaded.value / trackFiles.length) * 100)

        return {
          ...file,
          buffer: audioBuffer
        }
      }
    })

    const concurrencyLimit = isMobile ? 4 : trackFiles.length

    const loadedTracks = await runWithConcurrencyLimit(tasks, concurrencyLimit)

    loadingStage.value = 'Carregando interface...'
    loadedTracks.forEach(t => {
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
        buffer: t.buffer,
        source: null,
        gain,
        panner,
        analyser,
        db: 0,
        pan: 0,
        mute: false,
        solo: false
      })

      duration.value = t.buffer.duration
    })
    loadingStage.value = 'Pronto!'

    isLoading.value = false
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

  async function runWithConcurrencyLimit(tasks, limit) {
    const results = []
    const executing = []

    for (const task of tasks) {
      const p = Promise.resolve().then(() => task())
      results.push(p)

      if (limit <= tasks.length) {
        const e = p.then(() => executing.splice(executing.indexOf(e), 1))
        executing.push(e)

        if (executing.length >= limit) {
          await Promise.race(executing)
        }
      }
    }

    return Promise.all(results)
  }

  return {
    tracks,
    isPlaying,
    duration,
    currentTime,
    loadingStage,
    isLoading,
    loadProgress,
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