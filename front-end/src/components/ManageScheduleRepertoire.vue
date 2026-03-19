<template>
  <q-card>
    <q-bar v-if="!hideHeader" class="card-header">
      <div class="text-h6">Repertório</div>
      <q-space />
      <q-btn dense flat icon="fa fa-close" v-close-popup />
    </q-bar>

    <q-card-section>
      <div v-if="loading" class="row items-center justify-center q-pa-lg">
        <q-spinner-dots color="primary" />
      </div>

      <div v-else>
        <div v-if="!hideHeader" class="q-mb-md">
          <div class="text-subtitle2">Escala: {{ formatDate(schedule.date) }}</div>
          <div class="text-caption">Membros escalados:</div>
          <div class="row q-gutter-sm q-mt-sm">
            <q-chip v-for="m in schedule.assignedMembers" :key="m.id" dense>
              {{ m.name }} — {{ positionLabel(m.position) }}
            </q-chip>
            <div v-if="!schedule.assignedMembers || schedule.assignedMembers.length === 0" class="text-caption">Nenhum membro atribuído ainda.</div>
          </div>
        </div>

        <div class="q-mb-md">
          <div class="text-subtitle2 q-mb-xs">Selecionar músicas</div>
          <q-select
            v-model="selectedTracks"
            dense
            outlined
            multiple
            use-chips
            stack-label
            :options="availableTracksOptions"
            option-value="id"
            option-label="label"
            map-options
            emit-value
            input-debounce="200"
            filter
            :filter-method="filterTracks"
            placeholder="Pesquisar e selecionar músicas"
          />
        </div>

        <div v-if="!hideFooter" class="row justify-end q-gutter-sm">
          <q-btn color="primary" label="Salvar" @click="save" :loading="saving" />
          <q-btn v-if="showTransition" color="secondary" label="Salvar e Avançar" @click="saveAndAdvance" :loading="savingAdvance" />
        </div>
      </div>
    </q-card-section>
  </q-card>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { Notify } from 'quasar'
import api from '../api'
import { PositionOptions } from '../constants/PositionOptions'

const props = defineProps({
  scheduleId: { type: Number, required: true },
  showTransition: { type: Boolean, default: true },
  hideHeader: { type: Boolean, default: false },
  hideFooter: { type: Boolean, default: false },
  showNotify: { type: Boolean, default: true }
})
const emit = defineEmits(['saved','advanced'])

defineExpose({ save })

const loading = ref(false)
const saving = ref(false)
const savingAdvance = ref(false)

const schedule = ref({ date: null, assignedMembers: [], repertoire: [], currentAssignments: {} })
const availableTracks = ref([])
const selectedTracks = ref([])

const availableTracksOptions = computed(() => availableTracks.value.map(t => ({ id: t.id, label: `${t.title} ${t.author ? ' — ' + t.author : ''}` })))

function formatDate(d) {
  try { return new Date(d).toLocaleDateString(); } catch { return d }
}
function positionLabel(value) {
  const p = PositionOptions.find(x => x.value === Number(value))
  return p ? p.label : String(value)
}

function filterTracks(term, option) {
  if (!term) return true
  return option.label.toLowerCase().indexOf(term.toLowerCase()) !== -1
}

async function load() {
  loading.value = true
  try {
    const r = await api.get(`schedules/${props.scheduleId}/repertoire`);
    const dto = r.data
    schedule.value = {
      date: dto.date ?? dto.Date,
      assignedMembers: (dto.assignedMembers || dto.AssignedMembers || []).map(m => ({ id: m.id ?? m.Id, name: m.name ?? m.Name, position: m.position ?? m.Position })),
      repertoire: (dto.repertoire || dto.Repertoire || []).map(t => ({ id: t.id ?? t.Id, title: t.title ?? t.Title, author: t.author ?? t.Author }))
    }
    availableTracks.value = (dto.availableTracks || dto.AvailableTracks || []).map(t => ({ id: t.id ?? t.Id, title: t.title ?? t.Title, author: t.author ?? t.Author }))
    // initialize selectedTracks with existing repertoire track ids
    selectedTracks.value = schedule.value.repertoire.map(t => t.id)
  } catch (err) {
    console.error(err)
    Notify.create({ type: 'negative', message: 'Erro ao carregar repertório.' })
  } finally {
    loading.value = false
  }
}

async function save() {
  saving.value = true
  try {
    await api.post(`schedules/${props.scheduleId}/repertoire`, selectedTracks.value);
    if (props.showNotify) {
      Notify.create({ type: 'positive', message: 'Repertório salvo.' })
    }
    emit('saved', props.scheduleId)
  } catch (err) {
    console.error(err)
    Notify.create({ type: 'negative', message: 'Erro ao salvar repertório.' })
  } finally {
    saving.value = false
  }
}

async function saveAndAdvance() {
  savingAdvance.value = true
  try {
    await save()
    // advance to next status (2 = awaiting repertoire release) - reuse existing transition route
    await api.post('schedules/transition', { scheduleIds: [props.scheduleId], newStatus: 3 })
    Notify.create({ type: 'positive', message: 'Escala avançada.' })
    emit('advanced', props.scheduleId)
  } catch (err) {
    console.error(err)
    Notify.create({ type: 'negative', message: 'Erro ao avançar.' })
  } finally {
    savingAdvance.value = false
  }
}

onMounted(load)
</script>

<style scoped>
.q-mb-md { margin-bottom: 16px; }
</style>