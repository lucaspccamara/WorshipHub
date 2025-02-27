<template>
  <q-layout
    view="hHh lpR fFf"
  >
    <!-- Cabeçalho fixo -->
    <AppHeader />

    <!-- Conteúdo principal -->
    <q-page-container>
      <q-page class="flex justify-center">
        <q-card class="main-card flex items-start justify-center">
          <router-view></router-view>
        </q-card>
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<script setup>
import AppHeader from "../components/AppHeader.vue";
</script>

<style lang="scss" scoped>
.main-card {
  width: 100%;
  max-width: 600px; /* Melhor adaptação para mobile-first */
  // height: auto;
  // min-height: 83vh;
}
</style>