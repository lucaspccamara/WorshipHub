<template>
  <q-card class="column full-height">
    <div class="row items-center q-pa-sm bg-white text-dark" style="border-bottom: 1px solid var(--q-separator)">
      <q-icon name="fa fa-calendar-check" size="xs" class="q-ml-sm q-mr-sm text-primary" />
      <div class="text-subtitle1 text-weight-bold">Gerenciar Escala <span class="text-weight-regular text-grey-7 q-ml-sm">{{ formattedDate }}</span></div>
      <q-space />
      <q-btn dense flat icon="fa fa-close" v-close-popup />
    </div>

    <q-tabs
      v-model="tab"
      dense
      class="bg-grey-1 text-grey-7"
      active-color="primary"
      indicator-color="primary"
      align="left"
      inline-label
    >
      <q-tab name="members" icon="fa fa-people-group" label="Integrantes" class="q-px-lg" />
      <q-tab name="repertoire" icon="fa fa-music" label="Repertório" class="q-px-lg" />
    </q-tabs>

    <q-separator />

    <div class="col overflow-hidden relative-position bg-white">
      <div v-show="tab === 'members'" class="absolute-full q-pa-md overflow-auto">
        <ManageSchedulePosition 
          ref="membersRef"
          :schedule-ids="[scheduleId]" 
          :show-transition="false"
          :hide-header="true"
          :hide-footer="true"
          :show-notify="false"
          @saved="onSaved"
        />
      </div>

      <div v-show="tab === 'repertoire'" class="absolute-full q-pa-md overflow-auto">
        <ManageScheduleRepertoire 
          ref="repertoireRef"
          :schedule-id="scheduleId" 
          :show-transition="false"
          :hide-header="true"
          :hide-footer="true"
          :show-notify="false"
          @saved="onSaved"
        />
      </div>
    </div>

    <q-separator />

    <q-card-actions align="right" class="q-pa-md bg-grey-1">
      <q-btn
        v-if="canNotify"
        flat
        label="Notificar Alterações"
        icon="fa fa-paper-plane"
        color="secondary"
        no-caps
        class="q-mr-auto"
        :loading="notifying"
        @click="sendNotification"
      >
        <q-tooltip>Notificar membros sobre as mudanças</q-tooltip>
      </q-btn>
      
      <q-btn 
        label="Salvar Alterações" 
        icon="fa fa-save"
        color="primary" 
        unelevated
        no-caps 
        class="q-px-md"
        :loading="savingAll"
        @click="saveAll"
      />
    </q-card-actions>
  </q-card>
</template>

<script setup>
import { ref, computed } from 'vue';
import { Notify, useQuasar } from 'quasar';
import ManageSchedulePosition from './ManageSchedulePosition.vue';
import ManageScheduleRepertoire from './ManageScheduleRepertoire.vue';
import api from '../api';
import { Role } from '../constants/Role';
import { useAuthStore } from '../stores/authStore';

const authStore = useAuthStore();
const canNotify = computed(() => authStore.hasAnyRole([Role.Admin, Role.Leader]));

const props = defineProps({
  scheduleId: { type: Number, required: true },
  scheduleDate: { type: String, required: false }
});

const emit = defineEmits(['updateScheduleList']);

const $q = useQuasar();
const tab = ref('members');
const notifying = ref(false);
const savingAll = ref(false);

const membersRef = ref(null);
const repertoireRef = ref(null);

const formattedDate = computed(() => {
  if (!props.scheduleDate) return '';
  try {
    const [year, month, day] = props.scheduleDate.split('/');
    return `${day}/${month}/${year}`;
  } catch {
    return props.scheduleDate;
  }
});

function onSaved() {
  emit('updateScheduleList');
}

async function saveAll() {
  savingAll.value = true;
  try {
    const promises = [];
    if (membersRef.value) {
      promises.push(membersRef.value.save());
    }
    if (repertoireRef.value) {
      promises.push(repertoireRef.value.save());
    }
    
    if (promises.length > 0) {
      await Promise.all(promises);
      Notify.create({ type: 'positive', message: 'Todas as alterações foram salvas.' });
    }
  } catch (err) {
    console.error(err);
    Notify.create({ type: 'negative', message: 'Erro ao salvar algumas alterações.' });
  } finally {
    savingAll.value = false;
  }
}

async function sendNotification() {
  $q.dialog({
    title: 'Confirmar Notificação',
    message: 'Deseja enviar uma notificação manual para todos os membros desta escala informando sobre as alterações?',
    cancel: true,
    persistent: true,
    ok: { label: 'Sim, Notificar', color: 'secondary', unelevated: true },
    cancel: { label: 'Cancelar', flat: true }
  }).onOk(async () => {
    notifying.value = true;
    try {
      await api.post(`schedules/${props.scheduleId}/notify`);
      Notify.create({ type: 'positive', message: 'Notificações enviadas com sucesso.' });
    } catch (err) {
      console.error(err);
      Notify.create({ type: 'negative', message: 'Erro ao enviar notificações.' });
    } finally {
      notifying.value = false;
    }
  });
}
</script>

<style scoped>
.h-full {
  height: 100%;
}
</style>
