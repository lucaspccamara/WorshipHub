<template>
  <div v-cloak style="background-color: #f1f2f3;">
    <AppTemplate />
  </div>
</template>
 
<script setup>
import AppTemplate from './pages/AppTemplate.vue';
</script>

<style scoped>
[v-cloak] > * { display: none }
[v-cloak]::before { content: "Loading..." }
</style>