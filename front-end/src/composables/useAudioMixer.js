import { ref } from 'vue'

const isMobile = /Android|iPhone|iPad|iPod/i.test(navigator.userAgent)
const isIOS = /iPhone|iPad|iPod/i.test(navigator.userAgent)

const AudioCtx = window.AudioContext || window.webkitAudioContext
const audioContext = new AudioCtx()

const DRIFT_THRESHOLD = isIOS ? 0.15 : 0.02 // 150ms no iOS, 20ms no resto
const SYNC_COOLDOWN_MS = 2000 // 2 segundos de intervalo entre syncs no iOS
const CHECK_INTERVAL_MS = 100 // Verificar drift a cada 100ms no iOS

const tracks = ref([])
const isPlaying = ref(false)
const duration = ref(0)
const currentTime = ref(0)

const loadingStage = ref('Iniciando Engine de Áudio...')
const isLoading = ref(false)
const loaded = ref(0)
const totalTracks = ref(0)
const loadProgress = ref(0)

const syncCount = ref(0)
const lastSyncTimes = new Map() // Controle de cooldown por trilha
let lastCheckTime = 0
let playStartTimeAtContext = 0 // Marca o momento exato do Play no hardware
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

      // Configuração de fidelidade visual por plataforma
      const isAndroid = /Android/i.test(navigator.userAgent)
      let fftSize = 2048 // Desktop padrão
      if (isIOS) fftSize = 256
      else if (isAndroid) fftSize = 512

      analyser.fftSize = fftSize
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

    // Alinhamento preventivo: Já deixa todas as trilhas na posição inicial para pre-buffer
    tracks.value.forEach(t => {
      t.audio.currentTime = currentTime.value
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

    playStartTimeAtContext = audioContext.currentTime

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
      const now = Date.now()
      const elapsedSinceStart = (audioContext.currentTime - playStartTimeAtContext) * 1000

      // Auto-Stop: Pausa a música ao chegar no final para evitar drift infinito
      if (master.ended || masterTime >= duration.value - 0.1) {
        stop()
        return
      }

      // Janela de Inércia: Nos primeiros 2 segundos, não corrigimos drift
      // Isso permite que o buffer se estabilize sem seeks agressivos na largada
      const isInInertiaWindow = elapsedSinceStart < 2000

      if (!isInInertiaWindow) {
        // Fase de Sincronia Sob Demanda (Smart Check)
        const shouldCheckSync = !isIOS || (now - lastCheckTime > CHECK_INTERVAL_MS)

        if (shouldCheckSync) {
          if (isIOS) lastCheckTime = now

          for (let i = 1; i < tracks.value.length; i++) {
            const track = tracks.value[i]
            const slave = track.audio

            if (track.mute) continue

            const diff = slave.currentTime - masterTime

            if (Math.abs(diff) > DRIFT_THRESHOLD) {
              if (isIOS) {
                const lastSync = lastSyncTimes.get(track.name) || 0
                if (now - lastSync < SYNC_COOLDOWN_MS) continue

                lastSyncTimes.set(track.name, now)
              }

              slave.currentTime = masterTime
              syncCount.value++
            }
          }
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
    syncCount,
    loadMockTracks,
    play,
    stop,
    seek,
    setDb,
    toggleMute,
    toggleSolo
  }
}