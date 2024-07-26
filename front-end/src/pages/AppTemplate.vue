<template>
  <q-layout view="hHh lpR fFf">
    <q-page-container>
      <app-header></app-header>

      <!-- Conteúdo da página -->
      <q-page class="q-pa-md">
        <div class="text-h4">Olá Fulano!</div>
        <div class="text-h6">Fique por dentro do que está por vir...</div>
        <q-layout style="max-width: 1250px; width: 90%; margin: 0 auto;">
          <q-card class="main-card">
            <router-view></router-view>
          </q-card>
        </q-layout>
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<script setup>
import AppHeader from '../components/AppHeader.vue';
</script>

<style lang="scss">
</style>
