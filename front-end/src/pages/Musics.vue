<template>
  <AppSectionHeader title="Músicas" icon="fa-solid fa-music">
    <template #actions>
      <q-btn v-if="hasRole" color="primary" icon="fa fa-square-plus" no-caps label="Cadastrar" @click="editMusic(0)" />
    </template>
  </AppSectionHeader>

  <MusicList
    ref="musicListRef"
    @edit="editMusic"
    @open-overview="openMusicOverview"
  />

  <q-dialog v-model="dialogManageMusic" maximized transition-show="slide-up" transition-hide="slide-down">
    <ManageMusic :musicId="selectedMusic" @updateMusicList="refreshList" @closeDialog="dialogManageMusic = false" />
  </q-dialog>

  <q-dialog
    v-model="dialogMusic"
    maximized
    transition-show="slide-up"
    transition-hide="slide-down"
  >
    <MusicOverview :musicId="selectedMusic" />
  </q-dialog>
</template>

<script setup>
import { ref } from 'vue';
import AppSectionHeader from '../components/AppSectionHeader.vue';
import MusicList from '../components/MusicList.vue';
import ManageMusic from '../components/ManageMusic.vue';
import MusicOverview from '../components/MusicOverview.vue';
import { Role } from '../constants/Role';
import { useAuthStore } from "../stores/authStore";

const authStore = useAuthStore();
const hasRole = authStore.hasAnyRole([Role.Admin, Role.Leader]);

const dialogManageMusic = ref(false);
const dialogMusic = ref(false);
const selectedMusic = ref(null);
const musicListRef = ref(null);

const editMusic = (musicId) => {
  selectedMusic.value = musicId;
  dialogManageMusic.value = true;
};

function openMusicOverview(musicId) {
  selectedMusic.value = musicId;
  dialogMusic.value = true;
}

function refreshList() {
  if (musicListRef.value) {
    musicListRef.value.getMusics();
  }
}
</script>