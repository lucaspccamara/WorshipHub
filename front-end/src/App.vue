<template>
  <div class="q-pa-md" v-cloak>
    <router-view></router-view>
  </div>
</template>
 
 <script setup>
</script>

<style scoped>
[v-cloak] > * { display: none }
[v-cloak]::before { content: "Loading..." }
</style>
