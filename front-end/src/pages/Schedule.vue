<template>
  <q-card-section class="card-header">
    <span class="text-h6 header-label">Lista de Escalas</span>
    <q-btn class="float-right left-icon" color="primary" icon="fa fa-square-plus" no-caps @click="openDialogCreateSchedule">Cadastrar Escala</q-btn>
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
          <q-btn class="float-right" color="primary" label="Pesquisar" no-caps @click="getSchedule" />
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
      @request="onRequest"
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

      <template v-slot:body-cell-id="props">
        <q-td :props="props">
          <q-btn size="sm" color="primary" icon="fa fa-pen-to-square" @click="openDialogManageSchedule(props.row.id)" round>
            <q-tooltip anchor="bottom middle" self="top middle" :offset="[10, 10]" transition-show="scale" transition-hide="scale">Editar / Visualizar</q-tooltip>
          </q-btn>
          <q-btn class="q-ml-sm" size="sm" color="red" icon="fa fa-trash" @click="() => {console.log(props.row.id)}" round>
            <q-tooltip anchor="bottom middle" self="top middle" :offset="[10, 10]" transition-show="scale" transition-hide="scale">Excluir</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </q-table>
  </div>

  <q-dialog v-model="dialogCreateSchedule" persistent class="lg-dialog">
    <CreateSchedule></CreateSchedule>
  </q-dialog>

  <q-dialog v-model="dialogManageSchedule" persistent class="lg-dialog">
    <ManageSchedule></ManageSchedule>
  </q-dialog>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import api from '../api';
import CreateSchedule from '../components/CreateSchedule.vue';
import ManageSchedule from '../components/ManageSchedule.vue';
import { ApiFilter, ApiPagination } from '../entities/ApiUtils';
import { EventTypes } from '../constants/EventTypes';
import { ScheduleStatus } from '../constants/ScheduleStatus';

const dialogCreateSchedule = ref(false);
const dialogManageSchedule = ref(false);
const selectedSchedule = ref(0);
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
  {name: 'id', label: 'Ações', field: 'id', align: 'center', sortable: false, width: '100px'}
];

function openDialogCreateSchedule() {
  dialogCreateSchedule.value = true;
};

function openDialogManageSchedule(idSchedule) {
  selectedSchedule.value = idSchedule;
  dialogManageSchedule.value = true;
};

function getSchedule() {
  filter.filters = {
    startDate: startDate.value,
    endDate: endDate.value,
    eventType: eventType.value
  }

  api.getPost('schedules/list', filter).then((response) => {
    rows.value.splice(0, rows.value.length, ...response.data.data);
    pagination.rowsNumber = response.data.totalRecords;
    console.log(response.data)
  })
};

function onRequest(props) {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;

  filter.page = page
  filter.length = rowsPerPage
  
  pagination.page = page
  pagination.rowsPerPage = rowsPerPage
}

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