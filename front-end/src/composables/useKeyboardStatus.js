import { ref, onMounted, onBeforeUnmount } from 'vue'
import { Platform } from 'quasar'

export function useKeyboardStatus() {

const isKeyboardOpen = ref(false)

const handleFocusIn = (e) => {
  if (
    Platform.is.mobile && // só ativa em mobile
    (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA')
  ) {
    isKeyboardOpen.value = true
  }
}

const handleFocusOut = (e) => {
  if (
    Platform.is.mobile &&
    (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA')
  ) {
    isKeyboardOpen.value = false
  }
}

onMounted(() => {
  window.addEventListener('focusin', handleFocusIn)
  window.addEventListener('focusout', handleFocusOut)
})

onBeforeUnmount(() => {
  window.removeEventListener('focusin', handleFocusIn)
  window.removeEventListener('focusout', handleFocusOut)
})

  return { isKeyboardOpen }
}
