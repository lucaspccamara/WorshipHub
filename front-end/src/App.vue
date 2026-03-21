<template>
  <div class="app-container">
    <AppTemplate/>
    <InstallAppPrompt />
  </div>
</template>

<script setup>
import { useRegisterSW } from 'virtual:pwa-register/vue';
import AppTemplate from "./pages/AppTemplate.vue";
import InstallAppPrompt from "./components/InstallAppPrompt.vue";

// Quando um novo Service Worker está disponível e assumiu o controle
// (graças ao skipWaiting + clients.claim no SW), esta callback é chamada.
// Forçamos o reload para que o usuário receba os novos assets imediatamente.
useRegisterSW({
  onNeedRefresh() {
    window.location.reload();
  },
  // Intervalo de verificação de atualização a cada 60 minutos para apps
  // que ficam abertos por muito tempo em background (ex: PWA no Android).
  immediate: true,
});
</script>

<style scoped>
.app-container {
  background-color: #e7e7e7;
}
</style>