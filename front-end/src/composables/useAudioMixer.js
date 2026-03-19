import { ref } from 'vue'

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

// Web Worker para I/O fora da thread principal
const audioLoaderWorker = new Worker(
  new URL('../workers/audioLoader.worker.js', import.meta.url),
  { type: 'module' }
)

// Gerenciador de promessas para o Worker
let pendingWorkerRequests = new Map()

// Carregamento do AudioWorklet para Medidores Profissionais
let workletLoaded = null // Promessa de carregamento
async function ensureWorklet() {
  // 1. Verifica suporte básico no navegador
  if (!audioContext || !audioContext.audioWorklet) {
    console.warn('AudioWorklet: Não suportado neste ambiente. Usando AnalyserNode.')
    return false
  }
  
  if (workletLoaded) return workletLoaded
  
  workletLoaded = (async () => {
    try {
      // Usando caminho absoluto da pasta PUBLIC para evitar problemas de asset do Vite no dev server
      await audioContext.audioWorklet.addModule('/workers/meter-processor.js')
      console.log('AudioWorklet: MeterProcessor (Public) carregado.')
      return true
    } catch (e) {
      console.error('AudioWorklet: Falha crítica ao carregar módulo:', e)
      return false
    }
  })()
  
  return workletLoaded
}

audioLoaderWorker.onmessage = (e) => {
  const { type, requestId, results, error } = e.data
  const promise = pendingWorkerRequests.get(requestId)
  if (!promise) return

  if (type === 'SLICE_BATCH_SUCCESS') {
    promise.resolve(results)
  } else {
    promise.reject(error)
  }
  pendingWorkerRequests.delete(requestId)
}

function fetchSliceBatchFromWorker(trackNames, sliceIndex) {
  return new Promise((resolve, reject) => {
    const requestId = Math.random().toString(36).substring(7)
    pendingWorkerRequests.set(requestId, { resolve, reject })
    audioLoaderWorker.postMessage({
      type: 'FETCH_SLICE_BATCH',
      tracks: trackNames,
      sliceIndex,
      requestId
    })
  })
}

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

async function saveSlicesForTrack(trackName, slices) {
  const db = await initDB()
  return new Promise((resolve, reject) => {
    const transaction = db.transaction(STORE_NAME, 'readwrite')
    const store = transaction.objectStore(STORE_NAME)

    slices.forEach((slice, index) => {
      const key = `${trackName}_${index}`
      store.put(slice, key)
    })

    transaction.oncomplete = () => resolve()
    transaction.onerror = () => reject(transaction.error)
  })
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
   * Limpa todo o estado do mixer para carregar uma nova música.
   */
  function resetMixer() {
    stop()
    tracks.value.forEach(track => {
      if (track.gain) track.gain.disconnect()
      if (track.meterNode) track.meterNode.disconnect()
      if (track.analyser) track.analyser.disconnect()
    })
    tracks.value = []
    duration.value = 0
    currentTime.value = 0
    loaded.value = 0
    totalTracks.value = 0
    loadProgress.value = 0
    isLoading.value = false
    loadingStage.value = 'Iniciando Engine de Áudio...'
  }

  /**
   * --- CARREGAMENTO DINÂMICO ---
   * Recebe um array de tracks [{ name, url, order }] e processa em paralelo.
   */
  async function loadTracks(trackFiles) {
    if (!trackFiles || trackFiles.length === 0) return

    // Limpar estado anterior (caso esteja trocando de música)
    if (tracks.value.length) resetMixer()

    // Garante que o processador de medidores esteja carregado
    await ensureWorklet().catch(() => {})

    isLoading.value = true
    loadingStage.value = 'fetching'
    loadProgress.value = 0

    await clearOldCache()

    totalTracks.value = trackFiles.length

    // Processador de Fila com Concorrência
    const queue = [...trackFiles]
    const CONCURRENCY = 4

    const updateLoadingMessage = () => {
      const progress = loadProgress.value
      if (progress < 25) loadingStage.value = 'Iniciando preparação dos instrumentos...'
      else if (progress < 50) loadingStage.value = 'Otimizando trilhas de áudio...'
      else if (progress < 75) loadingStage.value = 'Configurando mixagem e efeitos...'
      else if (progress < 100) loadingStage.value = 'Finalizando últimos detalhes...'
    }

    const processTrack = async (file) => {
      try {
        const response = await fetch(file.url)
        const arrayBuffer = await response.arrayBuffer()

        let audioBuffer = await audioContext.decodeAudioData(arrayBuffer)

        if (audioBuffer.duration > duration.value) {
          duration.value = audioBuffer.duration
        }

        const numSlices = Math.ceil(audioBuffer.duration / SLICE_DURATION)
        const slicesToSave = []

        for (let i = 0; i < numSlices; i++) {
          const start = i * SLICE_DURATION
          const end = Math.min(start + SLICE_DURATION, audioBuffer.duration)
          const length = Math.floor((end - start) * audioBuffer.sampleRate)

          if (length <= 0) continue

          const channels = []
          for (let ch = 0; ch < audioBuffer.numberOfChannels; ch++) {
            const sliceData = audioBuffer.getChannelData(ch).slice(
              Math.floor(start * audioBuffer.sampleRate),
              Math.floor(end * audioBuffer.sampleRate)
            )
            channels.push(sliceData)
          }

          slicesToSave.push({
            numberOfChannels: audioBuffer.numberOfChannels,
            length: length,
            sampleRate: audioBuffer.sampleRate,
            channels: channels
          })
        }

        audioBuffer = null
        await saveSlicesForTrack(file.name, slicesToSave)

        const gain = audioContext.createGain()
        gain.gain.value = 1

        // Tenta usar AudioWorkletNode se disponível e carregado (Exige HTTPS/Localhost)
        let meterNode = null
        let analyser = null
        
        const isWorkletReady = await workletLoaded 
        
        if (audioContext.audioWorklet && isWorkletReady) {
          try {
            meterNode = new AudioWorkletNode(audioContext, 'meter-processor')
            gain.connect(meterNode)
            // NOTA: Não conectamos meterNode ao destination para evitar processamento extra de saída
          } catch (e) {
            console.warn('AudioWorkletNode: Erro na criação, fallback para AnalyserNode:', e)
          }
        }

        if (!meterNode) {
          analyser = audioContext.createAnalyser()
          analyser.fftSize = 2048 // Aumentado de 1024 para cobrir frames de 30fps (33ms) com folga
          gain.connect(analyser)
        }

        // Conexão Direta do Áudio (Garante que o som saia independente do medidor)
        gain.connect(audioContext.destination)

        gain.gain.value = 1

        tracks.value.push({
          name: file.name,
          order: file.order,
          gain,
          meterNode, 
          analyser, // Exportamos ambos para o MeterCanvas decidir qual usar
          db: 0,
          mute: false,
          solo: false,
          activeSources: []
        })

        tracks.value.sort((a, b) => a.order - b.order)

        loaded.value++
        loadProgress.value = Math.round((loaded.value / totalTracks.value) * 100)
        updateLoadingMessage()
      } catch (err) {
        console.error(`Erro em ${file.name}:`, err)
        loaded.value++
        loadProgress.value = Math.round((loaded.value / totalTracks.value) * 100)
        updateLoadingMessage()
      }
    }

    // Modelo de Workers Paralelos
    const workers = Array(CONCURRENCY).fill(null).map(async () => {
      while (queue.length > 0) {
        const file = queue.shift()
        if (file) await processTrack(file)
      }
    })

    await Promise.all(workers)

    loadingStage.value = 'Pronto!'
    isLoading.value = false
  }

  async function scheduleNextSlice(offsetInSong) {
    if (!isPlaying.value) return

    const sliceIndex = Math.floor(offsetInSong / SLICE_DURATION)
    const offsetInSlice = offsetInSong % SLICE_DURATION

    if (offsetInSong >= duration.value) {
      stop()
      return
    }

    // Busca os dados de TODAS as trilhas via Worker (Thread secundária)
    const trackNames = tracks.value.map(t => t.name)
    const results = await fetchSliceBatchFromWorker(trackNames, sliceIndex)
    
    if (!isPlaying.value) return

    const durations = tracks.value.map((track) => {
      const match = results.find(r => r.trackName === track.name)
      const sliceData = match?.data
      
      if (!sliceData) return 0

      // Reconstrói o AudioBuffer na thread principal (rápido)
      const buffer = audioContext.createBuffer(
        sliceData.numberOfChannels,
        sliceData.length,
        sliceData.sampleRate
      )
      for (let i = 0; i < sliceData.numberOfChannels; i++) {
        buffer.copyToChannel(sliceData.channels[i], i)
      }

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
    expectedNextSliceTime += currentSliceRemaining
    const nextOffsetInSong = offsetInSong + currentSliceRemaining

    // Look-ahead de 2 segundos
    const msToWait = (expectedNextSliceTime - audioContext.currentTime - 2) * 1000

    nextSliceTimeout = setTimeout(() => {
      scheduleNextSlice(nextOffsetInSong)
    }, Math.max(0, msToWait))
  }

  async function play(offset = currentTime.value) {
    if (isPlaying.value) return
    if (audioContext.state === 'suspended') await audioContext.resume()

    isPlaying.value = true

    // Fase de pré-fetch via Worker
    const sliceIndex = Math.floor(offset / SLICE_DURATION)
    const offsetInSlice = offset % SLICE_DURATION

    // Busca os dados iniciais via Worker (Thread secundária)
    const trackNames = tracks.value.map(t => t.name)
    const results = await fetchSliceBatchFromWorker(trackNames, sliceIndex)

    // Se o usuário parou a música durante o carregamento
    if (!isPlaying.value) return

    // Define o ponto de partida absoluto no tempo do AudioContext após os buffers estarem na RAM
    const renderDelay = 0.2 
    expectedNextSliceTime = audioContext.currentTime + renderDelay
    playbackStartTime = expectedNextSliceTime - offset

    // Dispara todas as trilhas sincronizadas no futuro próximo
    const durations = tracks.value.map((track) => {
      const match = results.find(r => r.trackName === track.name)
      const sliceData = match?.data
      
      if (!sliceData) return 0

      // Reconstrói o AudioBuffer na thread principal (rápido)
      const buffer = audioContext.createBuffer(
        sliceData.numberOfChannels,
        sliceData.length,
        sliceData.sampleRate
      )
      for (let i = 0; i < sliceData.numberOfChannels; i++) {
        buffer.copyToChannel(sliceData.channels[i], i)
      }

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

    // Look-ahead de 2 segundos
    const msToWait = (expectedNextSliceTime - audioContext.currentTime - 2) * 1000
    nextSliceTimeout = setTimeout(() => {
      scheduleNextSlice(nextOffsetInSong)
    }, Math.max(0, msToWait))

    updateTime()
  }

  let lastUpdateTime = 0
  function updateTime() {
    if (!isPlaying.value) return
    
    const now = performance.now()
    // Throttling: atualiza a reatividade do Vue ~16 vezes por segundo (a cada 62ms)
    // Isso é suficiente para o MiniPlayer mas poupa muita CPU no mobile
    if (now - lastUpdateTime > 60) {
      currentTime.value = audioContext.currentTime - playbackStartTime
      lastUpdateTime = now
    }

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
    const isSoloed = tracks.value.some(t => t.solo)
    tracks.value.forEach(track => {
      let targetGain = 0
      if (isSoloed) {
        targetGain = track.solo ? dbToGain(track.db) : 0
      } else {
        targetGain = track.mute ? 0 : dbToGain(track.db)
      }
      track.gain.gain.setTargetAtTime(targetGain, audioContext.currentTime, 0.05)
    })
  }

  function updateSingleTrackRouting(track) {
    const isSoloed = tracks.value.some(t => t.solo)
    let targetGain = 0
    if (isSoloed) {
      targetGain = track.solo ? dbToGain(track.db) : 0
    } else {
      targetGain = track.mute ? 0 : dbToGain(track.db)
    }
    track.gain.gain.setTargetAtTime(targetGain, audioContext.currentTime, 0.05)
  }

  function setDb(track, db) {
    track.db = Number(db.toFixed(1)) // Arredonda e previne reatividade excessiva de floats longos
    updateSingleTrackRouting(track)
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
    loadTracks,
    resetMixer,
    play,
    stop,
    seek,
    setDb,
    toggleMute,
    toggleSolo
  }
}