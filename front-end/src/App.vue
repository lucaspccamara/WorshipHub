<template>
  <div class="q-pa-md" v-cloak style="background-color: #f1f2f3;">
    <router-view></router-view>
  </div>
</template>
 
<script setup>
</script>

<style scoped>
[v-cloak] > * { display: none }
[v-cloak]::before { content: "Loading..." }
</style>
