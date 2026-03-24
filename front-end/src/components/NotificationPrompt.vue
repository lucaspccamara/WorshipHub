<template>
  <q-dialog v-model="showPrompt" position="bottom" seamless>
    <q-card style="width: 100%; border-radius: 16px 16px 0 0;" class="bg-secondary text-white notify-card">
      <div class="row items-center q-pa-md pb-0">
        <q-icon name="fa-solid fa-bell" size="3.5rem" class="q-mr-md" />
        <div class="col">
          <div class="text-h6 text-weight-bold" style="line-height: 1.2;">Ative as Notificações</div>
          <div class="text-caption q-mt-xs text-weight-medium" style="opacity: 0.9;">
            Fique por dentro das escalas, comunicados e atualizações em tempo real.
          </div>
        </div>
      </div>
      
      <q-card-actions align="right" class="q-pt-sm q-pb-md q-px-md">
        <q-btn flat label="Mais Tarde" color="white" class="opacity-80" @click="dismissPrompt" />
        <q-btn flat rounded label="Ativar" color="secondary" class="bg-white text-weight-bold q-px-md" @click="enableNotifications" />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useNotifications } from '../composables/useNotifications'
import { useAuthStore } from '../stores/authStore'
import { isInstallPromptVisible } from '../composables/useUIState'

const showPrompt = ref(false)
const { isSupported, permissionGranted, requestPermission } = useNotifications()
const authStore = useAuthStore()

const checkAndShowPrompt = () => {
  // Apenas tentar exibir se:
  // 1. Há suporte
  // 2. Não tem permissão ainda
  // 3. Usuário está logado
  if (!isSupported.value || permissionGranted.value || !authStore.isAuthenticated) {
    return
  }

  // Verifica se o browser suporta checar permissão e se a permissão não foi negada pelo SO
  const currentPermission = ('Notification' in window) ? Notification.permission : 'denied'
  if (currentPermission === 'denied' || currentPermission === 'granted') {
    return
  }

  const dismissData = localStorage.getItem('wh-notify-prompt-dismissed')
  let hasDismissed = false
  
  if (dismissData) {
    const dismissTime = parseInt(dismissData, 10)
    const sevenDaysInMs = 7 * 24 * 60 * 60 * 1000
    
    if (!isNaN(dismissTime) && (Date.now() - dismissTime < sevenDaysInMs)) {
      hasDismissed = true
    } else {
      localStorage.removeItem('wh-notify-prompt-dismissed')
    }
  }

  if (!hasDismissed) {
    // Atraso de 2 segundos para evitar poluição visual imediata com outros possíveis alertas no login
    setTimeout(() => {
      const currentPermissionAft = ('Notification' in window) ? Notification.permission : 'denied'
      if (authStore.isAuthenticated && currentPermissionAft === 'default') {
        if (!isInstallPromptVisible.value) {
          showPrompt.value = true
        } else {
          // Se o pop-up de instalação estiver visível, aguardar até que seja fechado
          const unwatch = watch(isInstallPromptVisible, (isVisible) => {
            if (!isVisible) {
              setTimeout(() => { showPrompt.value = true }, 500)
              unwatch()
            }
          })
        }
      }
    }, 2000)
  }
}

onMounted(() => {
  checkAndShowPrompt()
})

// Também observamos quando a pessoa logar, caso não haja refresh da página na home
watch(() => authStore.isAuthenticated, (newVal) => {
  if (newVal) {
    checkAndShowPrompt()
  } else {
    showPrompt.value = false
  }
})

const dismissPrompt = () => {
  showPrompt.value = false
  localStorage.setItem('wh-notify-prompt-dismissed', Date.now().toString())
}

const enableNotifications = async () => {
  showPrompt.value = false
  await requestPermission()
  
  // Se o usuário interagiu e no final recusou no prompt nativo ('denied')
  // não precisamos necessariamente salvar os 7 dias porque o navegador não deixará perguntar novamente de qualquer maneira.
  // Porém, por segurança, registramos que houve uma tomada de ação.
  const currentPermission = ('Notification' in window) ? Notification.permission : 'denied'
  if (currentPermission !== 'granted') {
     localStorage.setItem('wh-notify-prompt-dismissed', Date.now().toString())
  }
}
</script>

<style scoped>
.notify-card {
  box-shadow: 0 -4px 16px rgba(0, 0, 0, 0.2);
  padding-bottom: env(safe-area-inset-bottom);
}
.opacity-80 {
  opacity: 0.8;
}
</style>
