import { ref } from 'vue'

const isMobile = /Android|iPhone|iPad|iPod/i.test(navigator.userAgent)

const AudioCtx = window.AudioContext || window.webkitAudioContext
const audioContext = new AudioCtx()

const DRIFT_THRESHOLD = 0.02 // 20ms de tolerância antes de forçar sync

const tracks = ref([])
const isPlaying = ref(false)
const duration = ref(0)
const currentTime = ref(0)

const loadingStage = ref('Iniciando Engine de Áudio...')
const isLoading = ref(false)
const loaded = ref(0)
const totalTracks = ref(0)
const loadProgress = ref(0)

let animationFrameId = null

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

    const concurrencyLimit = isMobile ? 4 : trackFiles.length

    const tasks = trackFiles.map((file) => {
      return async () => {
        return new Promise((resolve) => {
          const audio = new Audio()
          audio.src = file.url
          audio.crossOrigin = 'anonymous'
          audio.preload = 'auto'

          const onCanPlay = async () => {
            if (audio.duration > duration.value) {
              duration.value = audio.duration
            }

            // 🔥 Priming: Inicializa o decodificador para evitar stutter no primeiro play
            try {
              audio.volume = 0
              await audio.play()
              audio.pause()
              audio.currentTime = 0
            } catch (e) {
              // Falha silenciosa se houver bloqueio de autoplay
            }

            loaded.value++
            loadProgress.value = Math.round((loaded.value / trackFiles.length) * 100)

            audio.removeEventListener('canplaythrough', onCanPlay)
            resolve({ ...file, audio })
          }

          audio.addEventListener('canplaythrough', onCanPlay)

          audio.onerror = () => {
            console.error(`Erro ao carregar trilha: ${file.name}`)
            loaded.value++
            resolve({ ...file, audio: null })
          }

          audio.load()
        })
      }
    })

    const loadedTracks = await runWithConcurrencyLimit(tasks, concurrencyLimit)

    loadingStage.value = 'Configurando roteamento...'

    loadedTracks.forEach(t => {
      if (!t || !t.audio) return

      const source = audioContext.createMediaElementSource(t.audio)
      const gain = audioContext.createGain()
      const analyser = audioContext.createAnalyser()

      analyser.fftSize = 512 // 512 oferece bom equilíbrio entre performance e visual
      analyser.smoothingTimeConstant = 0.8

      source.connect(gain)
      gain.connect(analyser)
      analyser.connect(audioContext.destination)

      tracks.value.push({
        name: t.name,
        audio: t.audio,
        gain,
        analyser,
        db: 0,
        mute: false,
        solo: false
      })
    })

    loadingStage.value = 'Pronto!'
    isLoading.value = false
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

      track.gain.gain.cancelScheduledValues(audioContext.currentTime)
      track.gain.gain.linearRampToValueAtTime(
        targetGain,
        audioContext.currentTime + 0.05
      )
    })
  }

  async function play(offset = currentTime.value) {
    if (isPlaying.value) return

    if (audioContext.state === 'suspended') {
      await audioContext.resume()
    }

    tracks.value.forEach(track => {
      track.audio.currentTime = offset
      track.audio.volume = 1
    })

    try {
      await Promise.all(tracks.value.map(track => track.audio.play()))

      isPlaying.value = true

      if (animationFrameId) cancelAnimationFrame(animationFrameId)
      animationFrameId = requestAnimationFrame(updateTime)
    } catch (err) {
      console.error("Erro ao iniciar reprodução:", err)
    }
  }

  function updateTime() {
    if (!isPlaying.value) return

    if (tracks.value.length > 0) {
      const master = tracks.value[0].audio
      const masterTime = master.currentTime
      currentTime.value = masterTime

      // Master-Slave Sync: Corrige o drift entre as faixas
      for (let i = 1; i < tracks.value.length; i++) {
        const slave = tracks.value[i].audio
        const diff = slave.currentTime - masterTime

        if (Math.abs(diff) > DRIFT_THRESHOLD) {
          // console.log(`Corrigindo drift na trilha ${tracks.value[i].name}: ${diff.toFixed(3)}s`)
          slave.currentTime = masterTime
        }
      }
    }

    animationFrameId = requestAnimationFrame(updateTime)
  }

  function stop() {
    tracks.value.forEach(track => {
      track.audio.pause()
    })
    isPlaying.value = false
    if (animationFrameId) cancelAnimationFrame(animationFrameId)
  }

  function seek(time) {
    const wasPlaying = isPlaying.value
    if (wasPlaying) stop()

    currentTime.value = time
    tracks.value.forEach(track => {
      track.audio.currentTime = time
    })

    if (wasPlaying) play(time)
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
    toggleSolo
  }
}