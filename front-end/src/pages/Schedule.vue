<template>
  <q-card-section class="card-header">
    <span class="text-h6 header-label">Lista de Escalas</span>
    <q-btn class="float-right left-icon" color="primary" no-caps icon="fa fa-square-plus" @click="openDialog">Cadastrar Escala</q-btn>
  </q-card-section>

  <q-card-section>
    <q-form>
      <div class="row q-gutter-md">
        <q-input v-model="startDate" outlined dense type="date" label="Escala de:" class="col-2" />
        <q-input v-model="endDate" outlined dense type="date" label="Escala até:" class="col-2" />
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

  <div class="q-pa-md">
    <q-table
      flat
      bordered
      row-key="id"
      color="primary"
      rows-pr-page-label="Registros por página"
      v-model:pagination="pagination"
      virtual-scroll
      :rows="rows"
      :columns="columns"
      :Loading="loading"
      separetor="cell"
      :rows-per-page-options="[5, 10, 15, 20, 25, 50, 100]"
    >
      <template v-slot:body-cell-eventType="props">
        <q-td :props="props">
          <span>{{ EventTypes.find(event => event.value == props.row.eventType).label }}</span>
        </q-td>
      </template>

      <template v-slot:body-cell-released="props">
        <q-td :props="props">
          <span>{{ ScheduleStatus.find(status => status.value == props.row.status).label }}</span>
        </q-td>
      </template>
    </q-table>
  </div>

  <q-dialog v-model="dialogRegister" persistent class="lg-dialog">
    <CreateSchedule></CreateSchedule>
  </q-dialog>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import api from '../api';
import CreateSchedule from '../components/CreateSchedule.vue';
import { ApiFilter, ApiPagination } from '../entities/ApiUtils';
import { EventTypes } from '../constants/EventTypes';
import { ScheduleStatus } from '../constants/ScheduleStatus';

const dialogRegister = ref(false);
let startDate = ref('');
let endDate = ref('');
let eventType = ref(null);
const filter = ApiFilter;
const pagination = ApiPagination;
const eventTypes = EventTypes;

const loading = ref(false);
const rows = ref([]);
const columns = [
  {name: 'date', label: 'Data', field: 'date', align: 'left', sortable: false},
  {name: 'eventType', label: 'Tipo de Evento', field: 'eventType', align: 'left', sortable: false},
  {name: 'released', label: 'Status', field: 'released', align: 'center', sortable: false},
  {name: 'id', label: '', field: 'id', align: 'center', sortable: false, width: '100px'}
];

function openDialog() {
  dialogRegister.value = true;
};

function getSchedule() {
  filter.filters = {
    startDate: startDate.value,
    endDate: endDate.value,
    eventType: eventType.value
  }

  api.getPost('schedules/list', filter).then((response) => {
    rows.value.splice(0, rows.value.length, ...response.data.data);
  })
};

function setDefaultDates() {
  const today = new Date();
  const firstDay = new Date(today.getFullYear(), today.getMonth(), 1).toISOString().split('T')[0];
  const lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0).toISOString().split('T')[0];
  
  startDate.value = firstDay;
  endDate.value = lastDay;
};

onMounted(() => {
  setDefaultDates();
  getSchedule();
});
</script>