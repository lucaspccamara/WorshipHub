<template>
  <div class="card-header">
    <span class="text-h6 header-label">Minhas Disponibilidades</span>
    <q-btn class="float-right" dense color="primary" icon="fa fa-refresh" @click="load" />
  </div>

  <div v-if="loading" class="row items-center justify-center q-pa-md">
    <q-spinner-dots size="30px" />
  </div>

  <div v-else class="row q-mt-md q-col-gutter-md flex-column justify-center">
    <div v-if="items.length === 0" class="q-pa-md text-center">
      Nenhuma solicitação de disponibilidade no momento.
    </div>

    <!-- mobile-first: cartão por evento (telinha pequena) -->
    <div v-for="item in items" :key="item.id" class="q-mb-md">
      <q-card>
        <q-card-section class="row items-center justify-center card-header">
          <div class="text-h6">{{ item.date }}</div>
        </q-card-section>

        <div class="row items-center justify-center">
          <div class="text-subtitle1">{{ EventTypes.find(et => et.value == item.eventType).label }}</div>
        </div>
        
        <q-card-actions align="between">
          <q-btn
            :class="{'responded-yes': item.available === true}"
            flat
            color="positive"
            label="Disponível"
            @click="onRespondClicked(item, true)"
            :loading="loadingMap[item.id]"
          />
          <q-btn
            :class="{'responded-no': item.available === false}"
            flat
            color="negative"
            label="Indisponível"
            @click="onRespondClicked(item, false)"
            :loading="loadingMap[item.id]"
          />
        </q-card-actions>
      </q-card>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import api from '../api'
import { Notify, Dialog } from 'quasar'
import { EventTypes } from '../constants/EventTypes'

const items = ref([]) // [{ id(int), eventType(int), eventDate(string), available(bool?) }]
const loading = ref(false)
const loadingMap = ref({}) // keyed by item.id for per-item loading

async function load() {
  loading.value = true
  try {
    api.get('schedules/availabilities/pending').then((response) => {
      items.value = response.data.data;
    })
  } catch (err) {
    Notify.create({ type: 'negative', message: 'Erro ao carregar solicitações.' })
  } finally {
    loading.value = false
  }
}

async function performRespond(id, available) {
  loadingMap.value = { ...loadingMap.value, [id]: true }
  try {
    await api.post('schedules/availabilities/respond', { id, available })
    // update local item
    const idx = items.value.findIndex(i => i.id === id)
    if (idx !== -1) items.value[idx].available = available
    Notify.create({ type: 'positive', message: 'Resposta enviada.' })
  } catch (err) {
    Notify.create({ type: 'negative', message: 'Erro ao enviar resposta.' })
  } finally {
    loadingMap.value = { ...loadingMap.value, [id]: false }
  }
}

async function onRespondClicked(item, chosenAvailable) {
  const prev = item.available
  // if no previous response -> just send
  if (prev === null || prev === undefined) {
    await performRespond(item.id, chosenAvailable)
    return
  }

  // if same choice as before -> do nothing (or optionally notify)
  if (prev === chosenAvailable) return

  // different choice: show confirmation dialog
  Dialog.create({
    title: 'Confirmar alteração',
    message: 'Você já respondeu esta solicitação. Deseja alterar sua resposta?',
    cancel: true,
    persistent: true,
    ok: { label: 'Sim', color: 'primary' },
    cancel: { label: 'Cancelar', color: 'negative' }
  }).onOk(() => {
    performRespond(item.id, chosenAvailable)
  })
}

onMounted(load)
</script>

<style scoped lang="scss">
.responded-yes {
  background-color: $positive;
  color: white !important;
}
.responded-no {
  background-color: $negative;
  color: white !important;
}
</style>