<template>
  <q-card>
    <q-bar v-if="!hideHeader" class="card-header">
      <div class="text-h6">Organização da Escala</div>
      <q-space />
      <q-btn dense flat icon="fa fa-close" v-close-popup>
        <q-tooltip>Fechar</q-tooltip>
      </q-btn>
    </q-bar>

    <q-card class="q-pa-sm" flat>
      <q-card-section class="q-pt-none">
        <div v-if="!hideHeader" class="text-subtitle2 q-mb-sm">Escala</div>

        <div v-if="loading" class="row items-center justify-center q-pa-lg">
          <q-spinner-dots size="30px" color="primary" />
        </div>

        <div v-else>
          <!-- Mobile-first: lista por schedule (cada schedule -> uma linha de data) -->
          <div class="visible-xs block q-pa-xs" v-if="$q.screen.lt.md">
            <div v-for="item in schedules" :key="item.scheduleId" class="q-mb-sm card-date">
              <div class="row items-center q-mb-xs">
                <div class="text-subtitle1">{{ formatDate(item.date) }}</div>
                <q-space />
                <q-badge v-if="hasAnyResponse()" color="primary" align="top" label="Respostas" />
              </div>

              <div v-for="pos in positionOptions" :key="pos.value" class="q-mb-xs">
                <div class="text-caption q-mb-xs">{{ pos.label }}</div>
                <q-select
                  dense
                  outlined
                  multiple
                  use-chips
                  :options="membersByPosition[pos.value] || []"
                  option-value="id"
                  option-label="name"
                  emit-value
                  map-options
                  :model-value="assignments[item.scheduleId]?.[pos.value] || []"
                  @update:model-value="val => onSelect(item.scheduleId, pos.value, val)"
                  :placeholder="`Selecionar ${pos.label}`"
                >
                  <template v-slot:option="scope">
                    <q-item v-bind="scope.itemProps">
                      <q-item-section avatar>
                        <span :class="availDotClass(getMemberAvailability(item.scheduleId, scope.opt.id))" class="avail-dot" />
                      </q-item-section>
                      <q-item-section>
                        <q-item-label>{{ scope.opt.name }}</q-item-label>
                        <q-item-label caption>{{ availLabel(getMemberAvailability(item.scheduleId, scope.opt.id)) }}</q-item-label>
                      </q-item-section>
                    </q-item>
                  </template>
                  <template v-slot:selected-item="scope">
                    <q-chip
                      :data-chip-id="`chip-${item.scheduleId}-${pos.value}-${scope.opt.id}`"
                      removable
                      dense
                      :color="getMemberAvailability(item.scheduleId, scope.opt.id) === false ? 'red-1' : undefined"
                      :text-color="getMemberAvailability(item.scheduleId, scope.opt.id) === false ? 'negative' : undefined"
                      :outlined="getMemberAvailability(item.scheduleId, scope.opt.id) === false"
                      @remove="scope.removeAtIndex(scope.index)"
                      class="q-ma-xs"
                    >
                      {{ scope.opt.name }}
                    </q-chip>
                  </template>
                </q-select>
              </div>
            </div>
          </div>

          <!-- Desktop/tablet: tabela com positions no header -->
          <div v-else class="q-mt-sm">
            <q-table
              flat
              :rows="tableRows"
              :columns="tableColumns"
              row-key="scheduleId"
              dense
              bordered
              separator="cell"
              :pagination="{ rowsPerPage: 0 }"
              hide-bottom
            >
              <template v-slot:body-cell="props">
                <q-td :props="props">
                  <div v-if="props.col.name === 'date'">
                    <div class="text-weight-bold">{{ formatDate(props.row.date) }}</div>
                    <div v-if="hasAnyResponse()" class="text-caption text-positive">Respostas</div>
                  </div>
                  <div v-else>
                    <q-select
                      dense
                      outlined
                      multiple
                      use-chips
                      :options="membersByPosition[props.col.name] || []"
                      option-value="id"
                      option-label="name"
                      emit-value
                      map-options
                      :model-value="assignments[props.row.scheduleId]?.[props.col.name] || []"
                      @update:model-value="val => onSelect(props.row.scheduleId, props.col.name, val)"
                      placeholder="Sel."
                    >
                      <template v-slot:option="scope">
                        <q-item v-bind="scope.itemProps">
                          <q-item-section avatar>
                            <span :class="availDotClass(getMemberAvailability(props.row.scheduleId, scope.opt.id))" class="avail-dot" />
                          </q-item-section>
                          <q-item-section>
                            <q-item-label>{{ scope.opt.name }}</q-item-label>
                            <q-item-label caption>{{ availLabel(getMemberAvailability(props.row.scheduleId, scope.opt.id)) }}</q-item-label>
                          </q-item-section>
                        </q-item>
                      </template>
                      <template v-slot:selected-item="scope">
                        <q-chip
                          :data-chip-id="`chip-${props.row.scheduleId}-${props.col.name}-${scope.opt.id}`"
                          removable
                          dense
                          :color="getMemberAvailability(props.row.scheduleId, scope.opt.id) === false ? 'red-1' : undefined"
                          :text-color="getMemberAvailability(props.row.scheduleId, scope.opt.id) === false ? 'negative' : undefined"
                          :outlined="getMemberAvailability(props.row.scheduleId, scope.opt.id) === false"
                          @remove="scope.removeAtIndex(scope.index)"
                          class="q-ma-xs"
                        >
                          {{ scope.opt.name }}
                        </q-chip>
                      </template>
                    </q-select>
                  </div>
                </q-td>
              </template>
            </q-table>
          </div>
        </div>
      </q-card-section>

      <q-card-actions v-if="!hideFooter" align="right">
        <q-btn color="primary" label="Salvar" @click="save" :loading="saving" />
        <q-btn v-if="showTransition" color="secondary" label="Salvar e Avançar" @click="saveAndAdvance" :loading="savingAdvance" />
      </q-card-actions>
    </q-card>
  </q-card>
</template>

<script setup>
import { ref, onMounted, computed, nextTick } from 'vue'
import { Notify, useQuasar } from 'quasar'
import api from '../api'
import { PositionOptions } from '../constants/PositionOptions'

const $q = useQuasar()

const props = defineProps({
  scheduleIds: { type: Array, required: false },
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
const positionOptions = PositionOptions

// data structures
const schedules = ref([]) // [{ scheduleId, date, eventType, status }]
const membersByPosition = ref({}) // { positionValue: [{id,name,...}] }
const assignments = ref({}) // { scheduleId: { positionValue: memberId[] } }
const availabilityBySchedule = ref({}) // { scheduleId: { userId: bool|null } }

// --- Availability helpers ---
function getMemberAvailability(scheduleId, userId) {
  const schedMap = availabilityBySchedule.value[scheduleId] ?? availabilityBySchedule.value[String(scheduleId)]
  if (!schedMap) return undefined // not collected for this schedule
  const id = Number(userId)
  if (!(id in schedMap) && !(String(id) in schedMap)) return undefined
  return schedMap[id] ?? schedMap[String(id)] // true | false | null
}

function availDotClass(avail) {
  if (avail === true) return 'avail-dot--yes'
  if (avail === false) return 'avail-dot--no'
  if (avail === null) return 'avail-dot--pending'
  return '' // undefined = availability not collected for this schedule
}

function availLabel(avail) {
  if (avail === true) return 'Disponível'
  if (avail === false) return 'Indisponível'
  if (avail === null) return 'Não respondeu'
  return ''
}

const tableColumns = computed(() => {
  const cols = [{ name: 'date', label: 'Data', align: 'left' }]
  positionOptions.forEach(p => cols.push({ name: p.value, label: p.label }))
  return cols
})
const tableRows = computed(() => (schedules.value || []).map(s => ({ scheduleId: s.scheduleId, date: s.date })))

function formatDate(d) {
  try { return new Date(d).toLocaleDateString(); } catch { return d }
}

function hasAnyResponse() {
  for (const posKey in membersByPosition.value) {
    const list = membersByPosition.value[posKey] || []
    if (list.some(x => x.available !== undefined && x.available !== null)) return true
  }
  return false
}

function memberResponded(memberId) {
  for (const posKey in membersByPosition.value) {
    const list = membersByPosition.value[posKey] || []
    const found = list.find(x => x.id === memberId)
    if (found) return found.available === true
  }
  return false
}

function isMemberUnavailable(memberId) {
  for (const posKey in membersByPosition.value) {
    const list = membersByPosition.value[posKey] || []
    const found = list.find(x => x.id === memberId)
    if (found) return found.available === false
  }
  return false
}

async function load() {
  loading.value = true
  try {
    const ids = (props.scheduleIds && props.scheduleIds.length) ? props.scheduleIds : []
    if (ids.length === 0) {
      Notify.create({ type: 'negative', message: 'Nenhuma escala informada.' })
      loading.value = false
      return
    }

    // request batch
    const resp = await api.post('schedules/assignments/details', ids)
    const dto = resp.data

    // schedules list
    const parsedSchedules = dto.schedules || dto.Schedules || []
    schedules.value = Array.isArray(parsedSchedules) 
      ? parsedSchedules.map(s => ({ scheduleId: s.scheduleId ?? s.ScheduleId, date: s.date ?? s.Date, eventType: s.eventType ?? s.EventType, status: s.status ?? s.Status }))
      : []

    // membersByPosition -> normalize and ensure arrays for all positions
    const raw = dto.membersByPosition || dto.MembersByPosition || {}
    const norm = {}
    // init all position keys so selects always have an array
    positionOptions.forEach(p => { norm[String(p.value)] = [] })

    for (const k in raw) {
      const arr = raw[k] || []
      norm[k] = Array.isArray(arr) ? arr.map(m => ({
        id: m.id ?? m.Id,
        name: m.name ?? m.Name ?? `Usuário ${m.id ?? m.Id}`,
        position: m.position ?? m.Position,
        phoneNumber: m.phoneNumber ?? m.PhoneNumber,
        avatarUrl: m.avatarUrl ?? m.AvatarUrl,
        available: (m.available ?? m.Available) ?? null
      })) : []
    }

    membersByPosition.value = norm

    // store per-schedule availability map
    const rawAvail = dto.availabilityBySchedule || dto.AvailabilityBySchedule || {}
    // normalize keys to numbers
    const normAvail = {}
    for (const sid in rawAvail) {
      normAvail[Number(sid)] = {}
      for (const uid in rawAvail[sid]) {
        const v = rawAvail[sid][uid]
        normAvail[Number(sid)][Number(uid)] = v === null ? null : Boolean(v)
      }
    }
    availabilityBySchedule.value = normAvail

    // init assignments from currentAssignments (per schedule) if provided, else empty
    assignments.value = {}
    const cur = dto.currentAssignments || dto.CurrentAssignments || {}
    for (const s of schedules.value) {
      const sid = String(s.scheduleId)
      const map = {}
      positionOptions.forEach(p => {
        const raw = cur[sid] ?? cur[s.scheduleId]
        const val = raw ? (raw[p.value] ?? raw[String(p.value)]) : null
        // normalize: API may return array or null
        map[p.value] = Array.isArray(val) ? val : (val != null ? [val] : [])
      })
      assignments.value[s.scheduleId] = map
    }

    // ensure assigned ids appear in options so select shows label (create placeholder if missing)
    for (const sid of Object.keys(assignments.value)) {
      const map = assignments.value[sid]
      for (const posKey of Object.keys(map)) {
        const assignedIds = map[posKey] || []
        const list = membersByPosition.value[String(posKey)] || []
        for (const assignedId of assignedIds) {
          if (!list.some(x => x.id === assignedId)) {
            list.push({ id: assignedId, name: `Usuário ${assignedId}`, position: Number(posKey), phoneNumber: '', avatarUrl: '', available: null })
            membersByPosition.value[String(posKey)] = list
          }
        }
      }
    }
  } catch (err) {
    Notify.create({ type: 'negative', message: 'Erro ao carregar dados.' })
  } finally {
    loading.value = false
  }
}

async function onSelect(scheduleId, position, newUserIds) {
  // newUserIds is an array because q-select is multiple
  const newIds = Array.isArray(newUserIds) ? newUserIds : (newUserIds != null ? [newUserIds] : [])

  // Only warn about newly added members that are unavailable FOR THIS SPECIFIC SCHEDULE
  const prev = assignments.value[scheduleId][position] || []
  const added = newIds.filter(id => !prev.includes(id))

  const unavailableAdded = added.filter(id => getMemberAvailability(scheduleId, id) === false)

  // Aceita provisoriamente a seleção para manter o Quasar em sincronia exata com o Vue.
  // Como o modal é assíncrono, se não fizermos isso, o Quasar altera a UI internamente,
  // mas o Vue ignora a reversão porque acha que o valor antigo não mudou.
  assignments.value = { ...assignments.value, [scheduleId]: { ...assignments.value[scheduleId], [position]: newIds } }

  if (unavailableAdded.length > 0) {
    const list = membersByPosition.value[String(position)] || membersByPosition.value[position] || []
    const name = (unavailableAdded || [])
      .map(id => list.find(m => m.id === id)?.name ?? `Usuário ${id}`)
      .join(', ')
    const confirmed = await new Promise(resolve => {
      $q.dialog({
        title: 'Confirmar escolha',
        message: `${name} está marcado como indisponível para esta data. Deseja mesmo atribuir?`,
        cancel: true,
        persistent: true,
        ok: { label: 'Sim', color: 'primary' },
        cancel: { label: 'Cancelar' }
      })
      .onOk(() => resolve(true))
      .onCancel(() => resolve(false))
      .onDismiss(() => resolve(false))
    })

    if (!confirmed) {
      // Reversão limpa usando a reatividade do Vue (o erro anterior ocorria pois
      // o Quasar abortava a execução da função inteira ao rejeitar a Promise do Dialog)
      await nextTick()
      const filteredIds = newIds.filter(id => !unavailableAdded.includes(id))
      const newAssignments = { ...assignments.value }
      newAssignments[scheduleId] = { ...newAssignments[scheduleId] }
      newAssignments[scheduleId][position] = filteredIds
      assignments.value = newAssignments
      return
    }
  }

  // commit locally (save happens only when user clicks Salvar)
  assignments.value = { ...assignments.value, [scheduleId]: { ...assignments.value[scheduleId], [position]: newIds } }
}

async function save() {
  saving.value = true
  try {
    const ids = (schedules.value || []).map(s => s.scheduleId)
    for (const sid of ids) {
      // send { positionValue: [userId, ...] } — only positions with at least one member
      const raw = assignments.value[sid] || {}
      const payload = { assignments: {} }
      for (const posKey of Object.keys(raw)) {
        const arr = raw[posKey]
        if (Array.isArray(arr) && arr.length > 0) {
          payload.assignments[posKey] = arr
        }
      }
      await api.post(`schedules/${sid}/assignments`, payload)
    }
    if (props.showNotify) {
      Notify.create({ type: 'positive', message: 'Atribuições salvas.' })
    }
    //emit('saved', ids)
  } catch (err) {
    Notify.create({ type: 'negative', message: 'Erro ao salvar.' })
  } finally {
    saving.value = false
  }
}

async function saveAndAdvance() {
  savingAdvance.value = true
  try {
    await save()
    const ids = (schedules.value || []).map(s => s.scheduleId)
    await api.post('schedules/transition', { scheduleIds: ids, newStatus: 2 })
    Notify.create({ type: 'positive', message: 'Escalas avançadas para aguardar repertório.' })
    emit('advanced', ids)
  } catch {
    Notify.create({ type: 'negative', message: 'Erro ao avançar escalas.' })
  } finally {
    savingAdvance.value = false
  }
}

onMounted(load)
</script>

<style scoped>
.card-date { border: 1px solid var(--q-separator); padding: 8px; border-radius:6px; }
@media (min-width: 600px) {
  .visible-xs { display: none; }
}

/* Availability indicator dot */
.avail-dot {
  display: inline-block;
  width: 10px;
  height: 10px;
  border-radius: 50%;
  border: 1.5px solid rgba(0,0,0,0.12);
  background: transparent;
  flex-shrink: 0;
}
.avail-dot--yes  { background: #4caf50; border-color: #388e3c; } /* green */
.avail-dot--no   { background: #f44336; border-color: #c62828; } /* red */
.avail-dot--pending { background: #9e9e9e; border-color: #616161; } /* gray */

.q-item__section--avatar {
  min-width: fit-content !important;
}
</style>