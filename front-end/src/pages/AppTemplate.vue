<template>
  <q-layout
    view="hHh lpR fFf"
  >
    <!-- Cabeçalho fixo -->
    <AppHeader />

    <!-- Conteúdo principal -->
    <q-page-container>
      <q-page class="flex">
        <div class="content flex content-start items-start justify-center">
          <router-view></router-view>
        </div>
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<script setup>
import AppHeader from "../components/AppHeader.vue";
</script>

<style lang="scss" scoped>
.content {
  width: 100%;
  max-width: 1200px;
  margin: 0 auto;
  background-color: #f1f2f3;
  box-shadow: 0 0 10px 2px rgba(0, 0, 0, 0.2), 0 0px 10px rgba(0, 0, 0, 0.24);
}
</style>