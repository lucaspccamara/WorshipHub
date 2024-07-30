<template>
  <div class="q-pa-md">
    <q-input v-model="date" filled type="date" hint="Native date" />
    <q-input v-model="date" filled type="date" hint="Native date" />
    <q-select v-model="eventType" :options="eventTypes" label="Tipo de Evento" />
    <q-btn @click="fetchEscalas" label="Consultar Escalas" />
    <!-- Lista de escalas -->
  </div>
</template>

<script setup>
import { ref } from 'vue';

const eventType = ref('');
const eventTypes = ['Reunião', 'Apresentação', 'Outro'];

const handleDayClick = (date) => {
  console.log('Data selecionada:', date);
};

const fetchEscalas = () => {
  // Aqui você pode fazer a consulta das escalas com base nos filtros selecionados
  console.log('Consultando escalas...');
};
</script>
