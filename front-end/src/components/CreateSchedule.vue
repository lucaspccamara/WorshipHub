<template>
  <div>
    <q-date v-model="selectedDate" @dayclick="handleDayClick" />
    <form @submit.prevent="addEvent">
      <q-select v-model="eventType" :options="eventTypes" label="Tipo de Evento" />
      <q-btn type="submit" label="Adicionar Evento" />
    </form>
  </div>
</template>

<script setup>
import { ref } from 'vue';

const selectedDate = ref(null);
const eventType = ref('');
const eventTypes = ['Reunião', 'Apresentação', 'Outro'];

const handleDayClick = (date) => {
  console.log('Data selecionada:', date);
};

const addEvent = () => {
  console.log('Evento adicionado:', eventType.value);
};
</script>
