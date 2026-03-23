<template>
  <q-dialog v-model="showPrompt" position="bottom" seamless>
    <q-card style="width: 100%; border-radius: 16px 16px 0 0;" class="bg-primary text-white install-card">
      <div class="row items-center q-pa-md pb-0">
        <q-icon name="fa-solid fa-download" size="3.5rem" class="q-mr-md" />
        <div class="col">
          <div class="text-h6 text-weight-bold" style="line-height: 1.2;">Instale o App</div>
          <div class="text-caption q-mt-xs text-weight-medium" v-if="!isIOS" style="opacity: 0.9;">
            Acesso rápido e melhor experiência de uso na sua tela inicial.
          </div>
          <div class="text-caption q-mt-xs text-weight-medium" v-else style="opacity: 0.9;">
            Para instalar, toque em <q-icon name="fa-solid fa-arrow-up-from-bracket" size="sm" class="q-mx-xs" /> e selecione <br><strong>Adicionar à Tela de Início</strong>.
          </div>
        </div>
      </div>
      
      <q-card-actions align="right" class="q-pt-sm q-pb-md q-px-md">
        <q-btn flat label="Mais Tarde" color="white" class="opacity-80" @click="dismissPrompt" />
        <q-btn v-if="!isIOS" flat rounded label="Instalar" color="primary" class="bg-white text-weight-bold q-px-md" @click="installApp" />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { isInstallPromptVisible } from '../composables/useUIState'

const showPrompt = isInstallPromptVisible
const deferredPrompt = ref(null)
const isIOS = ref(false)

onMounted(() => {
  // O app já está instalado (standalone)? Se sim, não faz nada.
  const isStandalone = window.matchMedia('(display-mode: standalone)').matches || window.navigator.standalone === true
  if (isStandalone) {
    return
  }
  const dismissData = localStorage.getItem('wh-pwa-prompt-dismissed')
  let hasDismissed = false
  
  if (dismissData) {
    const dismissTime = parseInt(dismissData, 10)
    const sevenDaysInMs = 7 * 24 * 60 * 60 * 1000
    
    if (!isNaN(dismissTime) && (Date.now() - dismissTime < sevenDaysInMs)) {
      hasDismissed = true
    } else {
      localStorage.removeItem('wh-pwa-prompt-dismissed')
    }
  }
  // Checa se o dispositivo é iOS
  const userAgent = window.navigator.userAgent.toLowerCase()
  isIOS.value = /iphone|ipad|ipod/.test(userAgent) || (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1)

  // Fluxo para Android / Desktop
  window.addEventListener('beforeinstallprompt', (e) => {
    // Previne que a barra de mini-info apareça nos navegadores modernos
    e.preventDefault()
    // Salva o evento para acionar depois
    deferredPrompt.value = e
    
    if (!hasDismissed) {
      setTimeout(() => {
        showPrompt.value = true
      }, 1000)
    }
  })

  // O iOS não emite o evento 'beforeinstallprompt', então verificamos manualmente
  // e mostramos a mensagem que instrui o usuário a fazer manualmente.
  if (isIOS.value) {
    if (!hasDismissed) {
      // Delay de alguns segundos para não ser intrusivo logo no primeiro load
      setTimeout(() => {
        showPrompt.value = true
      }, 1000)
    }
  }
})

const dismissPrompt = () => {
  showPrompt.value = false
  localStorage.setItem('wh-pwa-prompt-dismissed', Date.now().toString())
}

const installApp = async () => {
  if (deferredPrompt.value) {
    deferredPrompt.value.prompt()
    const { outcome } = await deferredPrompt.value.userChoice
    if (outcome === 'accepted') {
      console.log('User accepted the install prompt')
    } else {
      console.log('User dismissed the install prompt')
      localStorage.setItem('wh-pwa-prompt-dismissed', Date.now().toString())
    }
    deferredPrompt.value = null
    showPrompt.value = false
  }
}
</script>

<style scoped>
.install-card {
  box-shadow: 0 -4px 16px rgba(0, 0, 0, 0.2);
  /* Trata casos de safe-area em iPhones mais recentes para o bottom modal não ficar cortado */
  padding-bottom: env(safe-area-inset-bottom);
}
.opacity-80 {
  opacity: 0.8;
}
</style>
