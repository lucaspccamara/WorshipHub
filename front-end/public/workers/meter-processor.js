/**
 * meter-processor.js
 * AudioWorklet para cálculo de Pico e RMS em tempo real na thread de áudio.
 * Localizado na pasta PUBLIC para garantir acesso via URL estática.
 */
class MeterProcessor extends AudioWorkletProcessor {
  constructor() {
    super()
    this._peak = 0
    this._sumSquared = 0
    this._count = 0
  }

  process(inputs) {
    const input = inputs[0]
    if (input && input.length > 0) {
      const channelData = input[0]
      const length = channelData.length

      for (let i = 0; i < length; i++) {
        const sample = Math.abs(channelData[i])
        if (sample > this._peak) this._peak = sample
        this._sumSquared += sample * sample
        this._count++
      }

      if (this._count >= 768) {
        const rms = Math.sqrt(this._sumSquared / this._count)
        this.port.postMessage({ peak: this._peak, rms: rms })
        this._peak = 0
        this._sumSquared = 0
        this._count = 0
      }
    }
    return true
  }
}

registerProcessor('meter-processor', MeterProcessor)
