/**
 * audioLoader.worker.js
 * Web Worker para gerenciar o acesso ao IndexedDB e o carregamento de fatias de áudio
 * fora da thread principal, evitando engasgos na UI.
 */

const DB_NAME = 'WorshipHub_AudioCache'
const STORE_NAME = 'slices'

async function initDB() {
  return new Promise((resolve, reject) => {
    const request = indexedDB.open(DB_NAME, 1)
    request.onsuccess = () => resolve(request.result)
    request.onerror = () => reject(request.error)
  })
}

/**
 * Busca os dados brutos de uma fatia no IndexedDB
 */
async function getSliceData(trackName, index) {
  const db = await initDB()
  return new Promise((resolve, reject) => {
    const transaction = db.transaction(STORE_NAME, 'readonly')
    const store = transaction.objectStore(STORE_NAME)
    const key = `${trackName}_${index}`
    const request = store.get(key)
    request.onsuccess = () => resolve(request.result)
    request.onerror = () => reject(request.error)
  })
}

// Escuta mensagens da thread principal
self.onmessage = async (e) => {
  const { type, tracks, sliceIndex, requestId } = e.data

  if (type === 'FETCH_SLICE_BATCH') {
    try {
      const results = await Promise.all(
        tracks.map(async (trackName) => {
          const data = await getSliceData(trackName, sliceIndex)
          return { trackName, data }
        })
      )

      // Coleta todos os ArrayBuffers para transferência (performance)
      const transferables = []
      results.forEach(res => {
        if (res.data && res.data.channels) {
          res.data.channels.forEach(ch => {
            if (ch instanceof Float32Array) {
              transferables.push(ch.buffer)
            }
          })
        }
      })

      self.postMessage({
        type: 'SLICE_BATCH_SUCCESS',
        requestId,
        results
      }, transferables)
      
    } catch (error) {
      self.postMessage({
        type: 'SLICE_BATCH_ERROR',
        requestId,
        error: error.message
      })
    }
  }
}
