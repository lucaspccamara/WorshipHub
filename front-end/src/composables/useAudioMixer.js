import { ref } from 'vue'

const isIOS = /iPhone|iPad|iPod/i.test(navigator.userAgent)

const AudioCtx = window.AudioContext || window.webkitAudioContext
const audioContext = new AudioCtx()

const SLICE_DURATION = 15 // Segundos por fatia
const DB_NAME = 'WorshipHub_AudioCache'
const DB_VERSION = 1
const STORE_NAME = 'slices'

// Refs de Estado do Mixer
const tracks = ref([])
const isPlaying = ref(false)
const duration = ref(0)
const currentTime = ref(0)

// Refs de Loading
const loadingStage = ref('Iniciando Engine de Áudio...')
const isLoading = ref(false)
const loaded = ref(0)
const totalTracks = ref(0)
const loadProgress = ref(0)

// Estado Interno do Scheduler
let animationFrameId = null
let nextSliceTimeout = null
let playbackStartTime = 0 // Referência absoluta (audioContext.currentTime - offset)
let expectedNextSliceTime = 0 // Quando a próxima fatia deve começar

/**
 * --- INDEXED DB ENGINE ---
 * Gerencia o armazenamento persistente dos slices decodificados
 */
async function initDB() {
  return new Promise((resolve, reject) => {
    const request = indexedDB.open(DB_NAME, DB_VERSION)
    request.onupgradeneeded = (e) => {
      const db = e.target.result
      if (!db.objectStoreNames.contains(STORE_NAME)) {
        db.createObjectStore(STORE_NAME)
      }
    }
    request.onsuccess = () => resolve(request.result)
    request.onerror = () => reject(request.error)
  })
}

async function saveSlice(trackName, index, audioBuffer) {
  const db = await initDB()

  // Extraímos os dados dos canais para salvar como arrays genéricos (IndexedDB aceita AudioBuffer direto em alguns browsers, mas ArrayBuffer é mais seguro)
  const channels = []
  for (let i = 0; i < audioBuffer.numberOfChannels; i++) {
    channels.push(audioBuffer.getChannelData(i))
  }

  const sliceData = {
    numberOfChannels: audioBuffer.numberOfChannels,
    length: audioBuffer.length,
    sampleRate: audioBuffer.sampleRate,
    channels: channels
  }

  return new Promise((resolve, reject) => {
    const transaction = db.transaction(STORE_NAME, 'readwrite')
    const store = transaction.objectStore(STORE_NAME)
    const key = `${trackName}_${index}`
    store.put(sliceData, key)
    transaction.oncomplete = () => resolve()
    transaction.onerror = () => reject(transaction.error)
  })
}

async function getSliceAsBuffer(trackName, index) {
  const db = await initDB()
  const sliceData = await new Promise((resolve, reject) => {
    const transaction = db.transaction(STORE_NAME, 'readonly')
    const store = transaction.objectStore(STORE_NAME)
    const key = `${trackName}_${index}`
    const request = store.get(key)
    request.onsuccess = () => resolve(request.result)
    request.onerror = () => reject(request.error)
  })

  if (!sliceData) return null

  const buffer = audioContext.createBuffer(
    sliceData.numberOfChannels,
    sliceData.length,
    sliceData.sampleRate
  )

  for (let i = 0; i < sliceData.numberOfChannels; i++) {
    buffer.copyToChannel(sliceData.channels[i], i)
  }

  return buffer
}

async function clearOldCache() {
  const db = await initDB()
  return new Promise((resolve, reject) => {
    const transaction = db.transaction(STORE_NAME, 'readwrite')
    transaction.objectStore(STORE_NAME).clear()
    transaction.oncomplete = () => resolve()
    transaction.onerror = () => reject(transaction.error)
  })
}

/**
 * --- HELPER DE GAIN ---
 */
function dbToGain(db) {
  if (db <= -60) return 0
  return Math.pow(10, db / 20)
}

export function useAudioMixer() {

  /**
   * --- CARREGAMENTO SEQUENCIAL (SAFE FOR IOS) ---
   * Baixa, decodifica e fatia trilha por trilha para não estourar a RAM
   */
  async function loadMockTracks() {
    if (tracks.value.length) return

    isLoading.value = true
    loadProgress.value = 0
    loadingStage.value = 'Limpando cache...'

    // TODO: Implementar lógica de persistência inteligente (manter até limite X de memória)
    // para evitar reprocessamento de músicas abertas recentemente.
    await clearOldCache()

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

    totalTracks.value = trackFiles.length

    for (const file of trackFiles) {
      loadingStage.value = `Processando: ${file.name}...`

      try {
        const response = await fetch(file.url)
        const arrayBuffer = await response.arrayBuffer()

        // Decodifica apenas uma trilha por vez
        const audioBuffer = await audioContext.decodeAudioData(arrayBuffer)

        if (audioBuffer.duration > duration.value) {
          duration.value = audioBuffer.duration
        }

        const numSlices = Math.ceil(audioBuffer.duration / SLICE_DURATION)
        for (let i = 0; i < numSlices; i++) {
          const start = i * SLICE_DURATION
          const end = Math.min(start + SLICE_DURATION, audioBuffer.duration)
          const length = Math.floor((end - start) * audioBuffer.sampleRate)

          if (length <= 0) continue

          const sliceBuffer = audioContext.createBuffer(
            audioBuffer.numberOfChannels,
            length,
            audioBuffer.sampleRate
          )

          for (let ch = 0; ch < audioBuffer.numberOfChannels; ch++) {
            const channelData = audioBuffer.getChannelData(ch).subarray(
              Math.floor(start * audioBuffer.sampleRate),
              Math.floor(end * audioBuffer.sampleRate)
            )
            sliceBuffer.copyToChannel(channelData, ch)
          }

          await saveSlice(file.name, i, sliceBuffer)
        }

        const gain = audioContext.createGain()
        const analyser = audioContext.createAnalyser()

        const isAndroid = /Android/i.test(navigator.userAgent)
        let fftSize = 2048
        if (isIOS) fftSize = 256
        else if (isAndroid) fftSize = 512
        analyser.fftSize = fftSize

        gain.connect(analyser)
        analyser.connect(audioContext.destination)

        tracks.value.push({
          name: file.name,
          gain,
          analyser,
          db: 0,
          mute: false,
          solo: false,
          activeSources: []
        })

        loaded.value++
        loadProgress.value = Math.round((loaded.value / totalTracks.value) * 100)

      } catch (err) {
        console.error(`Erro ao processar ${file.name}:`, err)
        loaded.value++
      }
    }

    loadingStage.value = 'Pronto!'
    isLoading.value = false
  }

  /**
   * --- SCHEDULER ENGINE ---
   */
  async function scheduleNextSlice(offsetInSong) {
    if (!isPlaying.value) return

    const sliceIndex = Math.floor(offsetInSong / SLICE_DURATION)
    const offsetInSlice = offsetInSong % SLICE_DURATION

    if (offsetInSong >= duration.value) {
      stop()
      return
    }

    const slicePromises = tracks.value.map(async (track) => {
      const buffer = await getSliceAsBuffer(track.name, sliceIndex)
      if (!buffer || !isPlaying.value) return null

      const source = audioContext.createBufferSource()
      source.buffer = buffer
      source.connect(track.gain)
      track.activeSources.push(source)

      const startTimeAt = expectedNextSliceTime
      source.start(startTimeAt, offsetInSlice)

      // Quando o buffer termina, removemos da lista de ativos
      source.onended = () => {
        track.activeSources = track.activeSources.filter(s => s !== source)
      }

      return buffer.duration - offsetInSlice
    })

    const durations = await Promise.all(slicePromises)
    const currentSliceRemaining = Math.max(...durations.filter(d => d !== null), 0)

    expectedNextSliceTime += currentSliceRemaining
    const nextOffsetInSong = offsetInSong + currentSliceRemaining

    const msToWait = (expectedNextSliceTime - audioContext.currentTime - 1) * 1000

    nextSliceTimeout = setTimeout(() => {
      scheduleNextSlice(nextOffsetInSong)
    }, Math.max(0, msToWait))
  }

  async function play(offset = currentTime.value) {
    if (isPlaying.value) return
    if (audioContext.state === 'suspended') await audioContext.resume()

    isPlaying.value = true

    // --- FASE DE PRE-FETCH ATÔMICO ---
    const sliceIndex = Math.floor(offset / SLICE_DURATION)
    const offsetInSlice = offset % SLICE_DURATION

    // Busca os buffers iniciais de TODAS as trilhas em paralelo antes de agendar
    const buffers = await Promise.all(
      tracks.value.map(t => getSliceAsBuffer(t.name, sliceIndex))
    )

    // Se o usuário parou a música durante o carregamento dos buffers
    if (!isPlaying.value) return

    // Define o ponto de partida absoluto no tempo do AudioContext após os buffers estarem na RAM
    const renderDelay = 0.1 // 100ms para o hardware processar o disparo
    expectedNextSliceTime = audioContext.currentTime + renderDelay
    playbackStartTime = expectedNextSliceTime - offset

    // Dispara todas as trilhas sincronizadas no futuro próximo
    const durations = tracks.value.map((track, i) => {
      const buffer = buffers[i]
      if (!buffer) return 0

      const source = audioContext.createBufferSource()
      source.buffer = buffer
      source.connect(track.gain)
      track.activeSources.push(source)

      source.start(expectedNextSliceTime, offsetInSlice)

      source.onended = () => {
        track.activeSources = track.activeSources.filter(s => s !== source)
      }

      return buffer.duration - offsetInSlice
    })

    const currentSliceRemaining = Math.max(...durations, 0)

    // Agenda a recursividade para o próximo bloco
    const nextOffsetInSong = offset + currentSliceRemaining
    expectedNextSliceTime += currentSliceRemaining

    const msToWait = (expectedNextSliceTime - audioContext.currentTime - 1) * 1000
    nextSliceTimeout = setTimeout(() => {
      scheduleNextSlice(nextOffsetInSong)
    }, Math.max(0, msToWait))

    updateTime()
  }

  function updateTime() {
    if (!isPlaying.value) return
    currentTime.value = audioContext.currentTime - playbackStartTime

    if (currentTime.value >= duration.value) {
      stop()
      return
    }

    animationFrameId = requestAnimationFrame(updateTime)
  }

  function stop() {
    isPlaying.value = false
    if (animationFrameId) cancelAnimationFrame(animationFrameId)
    if (nextSliceTimeout) {
      clearTimeout(nextSliceTimeout)
      nextSliceTimeout = null
    }

    tracks.value.forEach(track => {
      track.activeSources.forEach(s => {
        try { s.stop() } catch (e) { }
      })
      track.activeSources = []
    })
  }

  function seek(time) {
    const wasPlaying = isPlaying.value
    stop()
    currentTime.value = time
    if (wasPlaying) play(time)
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
      track.gain.gain.setTargetAtTime(targetGain, audioContext.currentTime, 0.05)
    })
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