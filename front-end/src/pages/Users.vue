<template>
  <div class="card-header">
    <span class="text-h6 header-label">Usuários</span>
    <q-btn class="float-right left-icon" color="primary" icon="fa fa-square-plus" no-caps @click="opendialogCreateUser">
      Cadastrar
    </q-btn>
  </div>

  <q-card class="card-content q-ma-md">
    <q-card-section>
      <q-form>
        <div class="row q-col-gutter-md">
          <div class="col-xs-12 col-sm-6 col-md-3">
            <q-input
              v-model="nome"
              label="Nome:"
              outlined
              dense
            />
          </div>
          <div class="col-xs-12 col-sm-6 col-md-3">
            <q-input
              v-model="email"
              label="Email:"
              outlined
              dense
            />
          </div>
          <div class="col-xs-12 col-sm-6 col-md-3">
            <q-select
              v-model="status"
              :options="StatusBooleanOptions"
              label="Status:"
              outlined
              dense
              emit-value
              map-options
            />
          </div>
          <div class="col">
            <q-btn
              class="float-right"
              color="primary"
              label="Pesquisar"
              no-caps
              @click="getUsers"
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
        style="cursor: pointer;"
        @click="openDialogProfile(props.row.id)"
        @mousedown="startHold(props.row)"
        @mouseup="clearHold"
        @mouseleave="clearHold"
        @touchstart="startHold(props.row)"
        @touchend="clearHold"
      >
        <q-td key="name">{{ props.row.name }}</q-td>
        <q-td key="email">{{ props.row.email }}</q-td>
        <q-td key="status" class="text-center">
          <q-badge :color="getStatusColor(props.row.status)"
            :label="StatusBooleanOptions.find(status => status.value == props.row.status)?.label" />
        </q-td>
      </q-tr>
    </template>
    </q-table>
  </div>

  <q-dialog v-model="dialogCreateUser" persistent class="lg-dialog">
    <CreateUser
      @updateUsersList="getUsers"
      @closeDialog="dialogCreateUser = false"
    />
  </q-dialog>

  <q-dialog v-model="dialogEditUser" persistent class="lg-dialog">
    <q-card>
      <Profile
        :userIdProp="selectedUser"
        @updateUsersList="getUsers"
        @closeDialog="dialogEditUser = false"
      />
    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import api from '../api';
import CreateUser from '../components/CreateUser.vue';
import Profile from '../components/Profile.vue';
import { ApiFilter, ApiPagination } from '../entities/ApiUtils';
import { StatusBooleanOptions } from '../constants/StatusBooleanOptions';

const dialogCreateUser = ref(false);
const dialogEditUser = ref(false);
const selectedUser = ref(0);
const holdTimer = ref(null);
let nome = ref('');
let email = ref('');
let status = ref(StatusBooleanOptions[0].value); // 'Ativo' by default
const filter = ApiFilter;
const pagination = ApiPagination;

const loading = ref(false);
const rows = ref([]);
const columns = [
  {name: 'name', label: 'Nome', field: 'name', align: 'left', sortable: false},
  {name: 'email', label: 'Email', field: 'email', align: 'left', sortable: false},
  {name: 'status', label: 'Status', field: 'status', align: 'center', sortable: false}
];

function startHold(row) {
  holdTimer.value = setTimeout(() => {
    openDialogProfile(row.id);
  }, 500);
}

function clearHold() {
  if (holdTimer.value) {
    clearTimeout(holdTimer.value);
    holdTimer.value = null;
  }
}

function getStatusColor(status) {
  const foundStatus = StatusBooleanOptions.find(s => s.value === status);
  return foundStatus ? foundStatus.color : "grey";
}

function getUsers() {
  filter.filters = {
    nome: nome.value,
    email: email.value,
    status: status.value
  }

  api.getPost('users/list', filter).then((response) => {
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

function opendialogCreateUser() {
  dialogCreateUser.value = true;
};

function openDialogProfile(userId) {
  selectedUser.value = userId;
  dialogEditUser.value = true;
};

onMounted(() => {
  getUsers();
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