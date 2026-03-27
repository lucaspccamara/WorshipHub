<template>
  <AppSectionHeader title="Escalas" icon="fa-solid fa-calendar">
    <template #actions>
      <q-btn
        v-if="authStore.hasAnyRole([Role.Admin, Role.Leader]) && selectedRows.length > 0 && selectedRows[0].status === EScheduleStatus.Criado"
        color="secondary"
        label="Iniciar Coleta"
        icon="fa fa-list-check"
        no-caps
        @click="startCollecting"
      />

      <q-btn
        v-if="authStore.hasAnyRole([Role.Admin, Role.Leader]) && selectedRows.length > 0 && selectedRows[0].status === EScheduleStatus.ColetandoDisponibilidade"
        color="secondary"
        label="Escalar Membros"
        icon="fa fa-people-group"
        no-caps
        @click="openDialogManageSchedulePosition"
      />

      <q-btn-dropdown
        v-if="authStore.hasAnyRole([Role.Admin, Role.Leader]) && selectedRows.length > 0 && selectedRows[0].status === EScheduleStatus.AguardandoRepertorio"
        color="secondary"
        label="Ações"
        icon="fa fa-bolt"
        no-caps
      >
        <q-list class="q-py-sm">
          <q-item
            clickable v-close-popup
            @click="openGroupedRepertoire"
          >
            <q-item-section avatar min-width="auto" class="q-pr-sm">
              <q-icon name="fa fa-calendar-check" color="primary" size="sm" />
            </q-item-section>
            <q-item-section>Gerenciar Escala</q-item-section>
          </q-item>
          
          <q-item
            clickable v-close-popup
            @click="finishSchedule"
          >
            <q-item-section avatar min-width="auto" class="q-pr-sm">
              <q-icon name="fa fa-check-circle" color="positive" size="sm" />
            </q-item-section>
            <q-item-section>Concluir Escala</q-item-section>
          </q-item>
        </q-list>
      </q-btn-dropdown>

      <q-btn
        v-if="authStore.hasAnyRole([Role.Admin, Role.Leader]) && selectedRows.length > 0 && selectedRows[0].status === EScheduleStatus.Concluido"
        label="Gerenciar Escala"
        icon="fa fa-calendar-check"
        color="secondary"
        no-caps
        @click="openGroupedRepertoire"
      />

      <q-btn
        v-if="authStore.hasAnyRole([Role.Admin, Role.Leader])"
        label="Cadastrar"
        icon="fa fa-square-plus"
        color="primary"
        no-caps
        @click="openDialogCreateSchedule"
      />
    </template>
  </AppSectionHeader>

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
    <template v-slot:header-selection="scope" />
    <template v-slot:body="props">
      <q-tr
        :props="props"
        :class="{'bg-blue-2': isSelected(props.row)}"
        style="cursor: pointer;"
        @click="openDialogScheduleByStatus(props.row.id, props.row.status, props.row.date, props.row.eventType)"
      >
        <q-td key="id" class="text-center">
          <q-checkbox v-model="selectedRows" :val="props.row" :disable="selectedRows.length > 0 && selectedRows[0].status != props.row.status" />
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

  <q-dialog v-model="dialogCreateSchedule" persistent :class="selectedScheduleIds.length > 0 && selectedScheduleIds[0] > 0 ? 'sm-dialog' : 'lg-dialog'">
    <CreateSchedule
      :schedule-id="selectedScheduleIds[0] || 0"
      :schedule-date="selectedScheduleDate"
      :schedule-event-type="selectedScheduleEvent"
      :schedule-status="selectedScheduleStatus"
      @closeDialog="dialogCreateSchedule = false"
      @updateScheduleList="getSchedule"
    ></CreateSchedule>
  </q-dialog>

  <q-dialog v-model="dialogManageSchedule" persistent class="lg-dialog">
    <ManageSchedule></ManageSchedule>
  </q-dialog>

  <q-dialog v-model="dialogPosition" maximized transition-show="slide-up" transition-hide="slide-down">
    <ManageSchedulePosition
      :schedule-ids="editingSelectedScheduleById"
      @saved="onSavedPosition"
      @advanced="onAdvanced"
      @closeDialog="editingSelectedScheduleById = []"
    />
  </q-dialog>

  <q-dialog v-model="dialogRepertoire" maximized transition-show="slide-up" transition-hide="slide-down">
    <ManageScheduleRepertoire
      :schedule-ids="editingSelectedScheduleById"
      @saved="onRepertoireReleased"
      @advanced="onAdvanced"
    />
  </q-dialog>

  <q-dialog v-model="dialogManageCompletedSchedule" maximized transition-show="slide-up" transition-hide="slide-down">
    <ManageCompletedSchedule
      :schedule-ids="selectedScheduleIds"
      :schedule-date="selectedScheduleDate"
      :status="selectedScheduleStatus"
      @updateScheduleList="getSchedule"
    />
  </q-dialog>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import AppSectionHeader from '../components/AppSectionHeader.vue';
import { Notify } from 'quasar';
import api from '../api';
import CreateSchedule from '../components/CreateSchedule.vue';
import ManageSchedule from '../components/ManageSchedule.vue';
import { ApiFilter, ApiPagination } from '../entities/ApiUtils';
import { EventTypes } from '../constants/EventTypes';
import { ScheduleStatus, EScheduleStatus } from '../constants/ScheduleStatus';
import ManageSchedulePosition from '../components/ManageSchedulePosition.vue';
import ManageScheduleRepertoire from '../components/ManageScheduleRepertoire.vue';
import ManageCompletedSchedule from '../components/ManageScheduleCompleteView.vue';
import { Role } from '../constants/Role';
import { useAuthStore } from '../stores/authStore';

const authStore = useAuthStore();

const dialogCreateSchedule = ref(false);
const dialogManageSchedule = ref(false);
const dialogPosition = ref(false);
const dialogRepertoire = ref(false);
const dialogManageCompletedSchedule = ref(false);
const editingSelectedScheduleById = ref([]);
const selectedScheduleIds = ref([]);
const selectedScheduleDate = ref(null);
const selectedScheduleEvent = ref(null);
const selectedScheduleStatus = ref(0);
const selectedRows = ref([]);
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

function getStatusColor(status) {
  const foundStatus = ScheduleStatus.find(s => s.value === status);
  return foundStatus ? foundStatus.color : "grey";
}

function getSchedule() {
  selectedRows.value = []
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

function openDialogScheduleByStatus(idSchedule, status, date, eventType) {
  if (status == EScheduleStatus.Criado) {
    openDialogCreateSchedule(idSchedule, date, eventType, status);
  } else if (status == EScheduleStatus.ColetandoDisponibilidade) {
    dialogPosition.value = true;
    editingSelectedScheduleById.value = [idSchedule];
  } else if (status == EScheduleStatus.AguardandoRepertorio) {
    if (authStore.hasAnyRole([Role.Admin, Role.Leader])) {
      const [day, month, year] = date.split('/');
      selectedScheduleIds.value = [idSchedule];
      selectedScheduleDate.value = `${year}/${month}/${day}`;
      selectedScheduleStatus.value = status;
      dialogManageCompletedSchedule.value = true;
    } else {
      dialogRepertoire.value = true;
      editingSelectedScheduleById.value = [idSchedule];
    }
  } else if (status == EScheduleStatus.Concluido) {
    const [day, month, year] = date.split('/');
    selectedScheduleIds.value = [idSchedule];
    selectedScheduleDate.value = `${year}/${month}/${day}`;
    selectedScheduleStatus.value = status;
    dialogManageCompletedSchedule.value = true;
  }
};

function openDialogCreateSchedule(idSchedule, date, eventType, status) {
  if (idSchedule && date && eventType !== null) {
    const [day, month, year] = date.split('/');
    selectedScheduleIds.value = [idSchedule];
    selectedScheduleDate.value = `${year}/${month}/${day}`;
    selectedScheduleEvent.value = eventType;
    selectedScheduleStatus.value = status;
  } else {
    selectedScheduleIds.value = [];
    selectedScheduleDate.value = null;
    selectedScheduleEvent.value = null;
    selectedScheduleStatus.value = 0;
  }

  dialogCreateSchedule.value = true;
};

function openDialogManageSchedulePosition() {
  dialogPosition.value = true;
  editingSelectedScheduleById.value = selectedRows.value.map(s => s.id);
};

function openGroupedRepertoire() {
  if (selectedRows.value.length === 0) return;
  
  if (authStore.hasAnyRole([Role.Admin, Role.Leader])) {
    selectedScheduleIds.value = selectedRows.value.map(s => s.id);
    selectedScheduleStatus.value = selectedRows.value[0].status;
    // Se for apenas 1, pegamos a data para o subtitulo. Se for mais, o componente tratará como "Múltiplas"
    if (selectedRows.value.length === 1) {
      const [day, month, year] = selectedRows.value[0].date.split('/');
      selectedScheduleDate.value = `${year}/${month}/${day}`;
    } else {
      selectedScheduleDate.value = null;
    }
    dialogManageCompletedSchedule.value = true;
  } else {
    dialogRepertoire.value = true;
    editingSelectedScheduleById.value = selectedRows.value.map(s => s.id);
  }
}

async function startCollecting() {
  if (!selectedRows.value.length) return
  try {
    await api.post(`schedules/transition`, { scheduleIds: selectedRows.value.map(s => s.id), newStatus: EScheduleStatus.ColetandoDisponibilidade })
    Notify.create({ type: 'positive', message: 'Escalas movidas para Coleta de Disponibilidade.' })
    selectedRows.value = []
    getSchedule()
  } catch {
    Notify.create({ type: 'negative', message: 'Erro ao iniciar coleta.' })
  }
}

async function startWaitingRepertoire() {
  if (!selectedRows.value.length) return
  try {
    await api.post(`schedules/transition`, { scheduleIds: selectedRows.value.map(s => s.id), newStatus: EScheduleStatus.AguardandoRepertorio })
    Notify.create({ type: 'positive', message: 'Escalas movidas para Aguardar Repertório.' })
    selectedRows.value = []
    getSchedule()
  } catch {
    Notify.create({ type: 'negative', message: 'Erro ao mover escalas.' })
  }
}

async function finishSchedule() {
  if (!selectedRows.value.length) return
  try {
    await api.post(`schedules/transition`, { scheduleIds: selectedRows.value.map(s => s.id), newStatus: EScheduleStatus.Concluido })
    Notify.create({ type: 'positive', message: 'Escalas concluídas com sucesso.' })
    selectedRows.value = []
    getSchedule()
  } catch {
    Notify.create({ type: 'negative', message: 'Erro ao concluir escalas.' })
  }
}

function onSavedPosition(id) {
  getSchedule()
}
function onAdvanced(id) {
  getSchedule()
}
function onRepertoireReleased(id) {
  getSchedule()
}

onMounted(() => {
  setDefaultDates();
  getSchedule();
});
</script>