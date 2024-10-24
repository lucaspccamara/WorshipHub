<template>
  <q-layout>
    <q-page-container>
      <app-header></app-header>
      <q-page class="q-pa-md">
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
.main-card {
  height: 90vh !important;
}
</style>