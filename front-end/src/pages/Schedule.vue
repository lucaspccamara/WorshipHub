<template>
  <q-card-section class="card-header">
    <span class="text-h6 header-label">Lista de Escalas</span>
    <q-btn class="float-right left-icon" color="primary" no-caps icon="fa fa-square-plus" @click="openDialog">Cadastrar Escala</q-btn>
  </q-card-section>

  <q-card-section>
    <q-form>
      <div class="row q-gutter-md">
        <q-input v-model="initialDate" outlined dense type="date" label="Escala de:" class="col-2" />
        <q-input v-model="finalDate" outlined dense type="date" label="Escala até:" class="col-2" />
        <q-select
          v-model="eventType"
          @update:model-value="val => eventType = val ? val.value : null"
          :options="eventTypes"
          map-options
          outlined
          dense
          clearable
          label="Tipo de Evento"
          class="col-2"
        />
        <div class="col">
          <q-btn color="primary" no-caps @click="getSchedule" label="Pesquisar" class="float-right" />
        </div>
      </div>
    </q-form>
  </q-card-section>

  <q-separator></q-separator>

  <!-- lista paginada -->

  <q-dialog v-model="dialogRegister" persistent class="lg-dialog">
    <CreateSchedule></CreateSchedule>
  </q-dialog>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import api from '../api';
import CreateSchedule from '../components/CreateSchedule.vue';

const dialogRegister = ref(false);
let initialDate = ref('');
let finalDate = ref('');
let eventType = ref(null);
const eventTypes = [
  {label: 'Culto Vespertino', value: 0, color: 'blue'},
  {label: 'Escola Dominical', value: 1, color: 'green'},
  {label: 'Evento Especial', value: 2, color: 'orange'}
];

function openDialog() {
  dialogRegister.value = true;
};

function getSchedule() {
  api.getAll(`escalas?initialDate=${initialDate.value}&finalDate=${finalDate.value}${eventType.value ? `&eventType=${eventType.value}` : ''}`).then((response) => {
    console.log(response.data)
  })
};

function setDefaultDates() {
  const today = new Date();
  const firstDay = new Date(today.getFullYear(), today.getMonth(), 1).toISOString().split('T')[0];
  const lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0).toISOString().split('T')[0];
  
  initialDate.value = firstDay;
  finalDate.value = lastDay;
}

onMounted(() => {
  setDefaultDates();
});
</script>