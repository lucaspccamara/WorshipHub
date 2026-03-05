<template>
  <div class="card-header">
    <span class="text-h6 header-label">Músicas</span>
    <q-btn v-if="hasRole" class="float-right left-icon" color="primary" icon="fa fa-square-plus" no-caps @click="editMusic(0)">
      Cadastrar
    </q-btn>
  </div>

  <q-card class="card-content q-ma-md">
    <q-card-section>
      <q-form>
        <div class="row q-col-gutter-md">
          <div class="col-xs-12 col-sm-6 col-md-3">
            <q-input
              v-model="title"
              outlined
              dense
              label="Título"
              @keyup.enter="getMusics"
              debounce="300"
            />
          </div>
          <div class="col-xs-12 col-sm-6 col-md-3">
            <q-input
              v-model="artist"
              outlined
              dense
              label="Artista"
              @keyup.enter="getMusics"
              debounce="300"
            />
          </div>
          <div class="col-xs-12 col-sm-6 col-md-3">
            <q-input
              v-model="album"
              outlined
              dense
              label="Álbum"
              @keyup.enter="getMusics"
              debounce="300"
            />
          </div>
          <div class="col">
            <q-btn
              class="float-right"
              color="primary"
              label="Pesquisar"
              no-caps
              @click="getMusics"
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
      :loading="loading"
      separetor="cell"
      :rows-per-page-options="[5, 10, 15, 20, 25, 50, 100]"
      :visible-columns="columns.filter(col => col.visible).map(col => col.name)"
      @request="onRequest"
    >
      <template v-slot:body="props">
        <q-tr :props="props" @click="openMusicOverview(props.row.id)" class="cursor-pointer">
          <q-td key="title">{{ props.row.title }}</q-td>
          <q-td key="artist">{{ props.row.artist }}</q-td>
          <q-td key="album">{{ props.row.album }}</q-td>
          <q-td key="actions" v-if="hasRole">
            <q-btn dense flat icon="fa fa-edit" @click="editMusic(props.row.id)" />
          </q-td>
        </q-tr>
      </template>
    </q-table>
  </div>

  <q-dialog v-model="dialogManageMusic" maximized transition-show="slide-up" transition-hide="slide-down">
    <ManageMusic :musicId="selectedMusic" @updateMusicList="getMusics" @closeDialog="dialogManageMusic = false" />
  </q-dialog>

  <q-dialog
    v-model="dialogMusic"
    maximized
    transition-show="slide-up"
    transition-hide="slide-down"
  >
    <MusicOverview :music="selectedMusic" />
  </q-dialog>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import api from "../api";
import { ApiFilter, ApiPagination } from '../entities/ApiUtils';
import ManageMusic from '../components/ManageMusic.vue';
import MusicOverview from '../components/MusicOverview.vue';
import { Role } from '../constants/Role';
import { useAuthStore } from "../stores/authStore";

const authStore = useAuthStore();
const hasRole = authStore.hasAnyRole([Role.Admin, Role.Leader]);

const title = ref('');
const artist = ref('');
const album = ref('');
const dialogManageMusic = ref(false);
const dialogMusic = ref(false);
const selectedMusic = ref(null);

const filter = ApiFilter;
const pagination = ApiPagination;

const loading = ref(false);
const rows = ref([]);
const columns = computed(() =>[
  { name: 'title', label: 'Título', field: 'title', align: 'left', visible: true },
  { name: 'artist', label: 'Artista', field: 'artist', align: 'left', visible: true },
  { name: 'album', label: 'Album', field: 'album', align: 'left', visible: true },
  { name: 'actions', label: 'Ações', field: 'actions', align: 'right', visible: hasRole }
]);

function getMusics() {
  filter.filters = {
    title: title.value,
    artist: artist.value,
    album: album.value
  }

  loading.value = true;
  api.post('musics/list', filter).then((response) => {
    rows.value.splice(0, rows.value.length, ...response.data.data);
    pagination.rowsNumber = response.data.totalRecords;
  }).finally(() => {
    loading.value = false;
  })
  .catch(() => {
    loading.value = false;
  });
};

function onRequest(props) {
  const { page, rowsPerPage } = props.pagination;

  filter.page = page
  filter.length = rowsPerPage

  pagination.page = page
  pagination.rowsPerPage = rowsPerPage
}

const editMusic = (musicId) => {
  selectedMusic.value = musicId;
  dialogManageMusic.value = true;
};

function openMusicOverview(musicId) {
  selectedMusic.value = musicId
  dialogMusic.value = true
}

onMounted(() => {
  getMusics();
});
</script>