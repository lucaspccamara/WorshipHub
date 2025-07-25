<template>
  <div class="card-header">
    <span class="text-h6 header-label">Escalas</span>
    <q-btn
      class="float-right left-icon"
      color="primary"
      icon="fa fa-square-plus"
      no-caps
      @click="openDialogCreateSchedule"
    >
      Cadastrar
    </q-btn>
  </div>

  <q-card class="card-content q-ma-md">
    <q-card-section>
      <q-form>
        <div class="row q-col-gutter-md">
          <div class="col-xs-6 col-sm-6 col-md-3">
            <q-input
              v-model="startDate"
              outlined
              dense
              type="date"
              label="Escala de:"
            />
          </div>
          <div class="col-xs-6 col-sm-6 col-md-3">
            <q-input
              v-model="endDate"
              outlined
              dense
              type="date"
              label="Escala até:"
            />
          </div> 
          <div class="col-xs-8 col-sm-6 col-md-3">
            <q-select
              v-model="eventType"
              @update:model-value="val => eventType = val ? val.value : null"
              :options="eventTypes"
              map-options
              outlined
              dense
              clearable
              label="Tipo de Evento"
            />
          </div>
          <div class="col">
            <q-btn
              class="float-right"
              color="primary"
              label="Pesquisar"
              no-caps
              @click="getSchedule"
            />
          </div>
        </div>
      </q-form>
    </q-card-section>
  </q-card>

  <!-- Ações em Massa -->
  <div class="q-pa-md">
    <q-btn 
      color="orange" 
      label="Liberar Selecionados" 
      icon="fa fa-play"
      :disable="selectedRows.length === 0" 
      @click="releaseSelectedSchedules"
    />
  </div>

  <div class="row full-width q-pa-md">
    <q-table
      class="no-select full-width"
      flat
      bordered
      row-key="id"
      color="primary"
      rows-pr-page-label="Registros por página"
      v-model:pagination="pagination"
      virtual-scroll
      selection="multiple"
      v-model:selected="selectedRows"
      :rows="rows"
      :columns="columns"
      :Loading="loading"
      separetor="cell"
      :rows-per-page-options="[5, 10, 15, 20, 25, 50, 100]"
      @request="onRequest"
    >
    <template v-slot:body="props">
      <q-tr 
        :props="props"
        :class="{'bg-blue-2': isSelected(props.row)}"
        style="cursor: pointer;"
        @dblclick="openDialogScheduleByStatus(props.row.id, props.row.status)"
        @mousedown="startHold(props.row)"
        @mouseup="clearHold"
        @mouseleave="clearHold"
        @touchstart="startHold(props.row)"
        @touchend="clearHold"
      >
        <q-td key="id" class="text-center">
          <q-checkbox v-model="selectedRows" :val="props.row" :disable="props.row.status != 0" />
        </q-td>
        <q-td key="date">{{ props.row.date }}</q-td>
        <q-td key="eventType">
          {{ eventTypes.find(event => event.value == props.row.eventType)?.label }}
        </q-td>
        <q-td key="status" class="text-center">
          <q-badge :color="getStatusColor(props.row.status)"
            :label="ScheduleStatus.find(status => status.value == props.row.status)?.label" />
        </q-td>
      </q-tr>
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
const selectedRows = ref([]);
const holdTimer = ref(null);
const isSelected = (row) => selectedRows.value.some(item => item.id === row.id);
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
  {name: 'status', label: 'Status', field: 'released', align: 'center', sortable: false}
];

function startHold(row) {
  holdTimer.value = setTimeout(() => {
    openDialogScheduleByStatus(row.id, row.status);
  }, 500);
}

function clearHold() {
  if (holdTimer.value) {
    clearTimeout(holdTimer.value);
    holdTimer.value = null;
  }
}

function getStatusColor(status) {
  const foundStatus = ScheduleStatus.find(s => s.value === status);
  return foundStatus ? foundStatus.color : "grey";
}

function getSchedule() {
  filter.filters = {
    startDate: startDate.value,
    endDate: endDate.value,
    eventType: eventType.value
  }

  api.getPost('schedules/list', filter).then((response) => {
    rows.value.splice(0, rows.value.length, ...response.data.data);
    pagination.rowsNumber = response.data.totalRecords;
  })
};

function onRequest(props) {
  const { page, rowsPerPage } = props.pagination;

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

function releaseSelectedSchedules() {
  if (selectedRows.value.length === 0) return;

  const schedulesToRelease = selectedRows.value.filter(row => !row.released);

  if (schedulesToRelease.length === 0) {
    alert("Nenhuma escala selecionada pode ser liberada!");
    return;
  }

  // Simulação da chamada para API
  schedulesToRelease.forEach(schedule => {
    schedule.released = true; // Atualiza localmente o status
  });

  alert("Escalas liberadas com sucesso!");
  
  // Limpa seleção após liberação
  selectedRows.value = [];
}

function openDialogScheduleByStatus(idSchedule, status) {
  if (status == 1) {
    openDialogManageSchedule(idSchedule)
  } else {
    openDialogCreateSchedule();
  }
};

function openDialogCreateSchedule() {
  dialogCreateSchedule.value = true;
};

function openDialogManageSchedule(idSchedule) {
  selectedSchedule.value = idSchedule;
  dialogManageSchedule.value = true;
};

onMounted(() => {
  setDefaultDates();
  getSchedule();
});
</script>

<style scoped>
.no-select {
  user-select: none;
  -webkit-user-select: none;
  -moz-user-select: none;
  -ms-user-select: none;
}
</style>