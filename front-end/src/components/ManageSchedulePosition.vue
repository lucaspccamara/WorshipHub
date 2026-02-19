<template>
  <q-card>
    <q-bar class="card-header">
      <div class="text-h6">Organização da Escala</div>
      <q-space />
      <q-btn dense flat icon="fa fa-close" v-close-popup>
        <q-tooltip>Fechar</q-tooltip>
      </q-btn>
    </q-bar>

    <q-card class="q-pa-sm">
      <q-card-section class="q-pt-none">
        <div class="text-subtitle2 q-mb-sm">Escala</div>

        <div v-if="loading" class="row items-center justify-center">
          <q-spinner-dots size="30px" />
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
                  :options="membersByPosition[pos.value] || []"
                  option-value="id"
                  option-label="name"
                  emit-value
                  map-options
                  :model-value="assignments[item.scheduleId][pos.value]"
                  @update:model-value="val => onSelect(item.scheduleId, pos.value, val)"
                  :placeholder="`Selecionar ${pos.label}`"
                >
                  <!-- <template #option="opt">
                    <div class="row items-center no-wrap">
                      <div>{{ opt.label }}</div>
                      <q-space />
                      <q-chip v-if="memberResponded(opt.value)" dense color="green" text-color="white" class="q-ml-xs">R</q-chip>
                      <q-chip v-else-if="isMemberUnavailable(opt.value)" dense color="negative" text-color="white" class="q-ml-xs">Indisp</q-chip>
                    </div>
                  </template> -->
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
            >
              <template v-slot:body-cell="props">
                <q-td :props="props">
                  <div v-if="props.col.name === 'date'">
                    {{ formatDate(props.row.date) }}
                    <div v-if="hasAnyResponse()" class="text-caption text-positive">Respostas</div>
                  </div>
                  <div v-else>
                    <q-select
                      dense
                      outlined
                      :options="membersByPosition[props.col.name] || []"
                      option-value="id"
                      option-label="name"
                      emit-value
                      map-options
                      :model-value="assignments[props.row.scheduleId][props.col.name]"
                      @update:model-value="val => onSelect(props.row.scheduleId, props.col.name, val)"
                      :placeholder="'Selecionar'"
                    >
                      <!-- <template #option="opt">
                        <div class="row items-center no-wrap">
                          <div>{{ opt.label }}</div>
                          <q-space />
                          <q-chip v-if="memberResponded(opt.value)" dense color="green" text-color="white" class="q-ml-xs">R</q-chip>
                          <q-chip v-else-if="isMemberUnavailable(opt.value)" dense color="negative" text-color="white" class="q-ml-xs">Indisp</q-chip>
                        </div>
                      </template> -->
                    </q-select>
                  </div>
                </q-td>
              </template>
            </q-table>
          </div>
        </div>
      </q-card-section>

      <q-card-actions align="right">
        <q-btn color="primary" label="Salvar" @click="save" :loading="saving" />
        <q-btn color="secondary" label="Salvar e Avançar" @click="saveAndAdvance" :loading="savingAdvance" />
      </q-card-actions>
    </q-card>
  </q-card>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { Notify, useQuasar } from 'quasar'
import api from '../api'
import { PositionOptions } from '../constants/PositionOptions'

const $q = useQuasar()

const props = defineProps({
  scheduleIds: { type: Array, required: false },
})
const emit = defineEmits(['saved','advanced'])

const loading = ref(false)
const saving = ref(false)
const savingAdvance = ref(false)
const positionOptions = PositionOptions

// data structures
const schedules = ref([]) // [{ scheduleId, date, eventType, status }]
const membersByPosition = ref({}) // { positionValue: [{id,name,available,...}] }
const assignments = ref({}) // { scheduleId: { positionValue: memberId|null } }

const tableColumns = computed(() => {
  const cols = [{ name: 'date', label: 'Data', align: 'left' }]
  positionOptions.forEach(p => cols.push({ name: p.value, label: p.label }))
  return cols
})
const tableRows = computed(() => schedules.value.map(s => ({ scheduleId: s.scheduleId, date: s.date })))

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
    schedules.value = (dto.schedules || dto.Schedules || []).map(s => ({ scheduleId: s.scheduleId ?? s.ScheduleId, date: s.date ?? s.Date, eventType: s.eventType ?? s.EventType, status: s.status ?? s.Status }))

    // membersByPosition -> normalize and ensure arrays for all positions
    const raw = dto.membersByPosition || dto.MembersByPosition || {}
    const norm = {}
    // init all position keys so selects always have an array
    positionOptions.forEach(p => { norm[String(p.value)] = [] })

    for (const k in raw) {
      norm[k] = (raw[k] || []).map(m => ({
        id: m.id ?? m.Id,
        name: m.name ?? m.Name ?? `Usuário ${m.id ?? m.Id}`,
        position: m.position ?? m.Position,
        phoneNumber: m.phoneNumber ?? m.PhoneNumber,
        avatarUrl: m.avatarUrl ?? m.AvatarUrl,
        available: (m.available ?? m.Available) ?? null
      }))
    }

    membersByPosition.value = norm

    // init assignments from currentAssignments (per schedule) if provided, else empty
    assignments.value = {}
    const cur = dto.currentAssignments || dto.CurrentAssignments || {}
    for (const s of schedules.value) {
      const sid = String(s.scheduleId)
      const map = {}
      positionOptions.forEach(p => {
        const val = (cur[sid] && (cur[sid][p.value] ?? cur[sid][String(p.value)])) ?? null
        map[p.value] = val
      })
      assignments.value[s.scheduleId] = map
    }

    // ensure assigned ids appear in options so select shows label (create placeholder if missing)
    for (const sid of Object.keys(assignments.value)) {
      const map = assignments.value[sid]
      for (const posKey of Object.keys(map)) {
        const assignedId = map[posKey]
        if (assignedId == null) continue
        const list = membersByPosition.value[String(posKey)] || []
        if (!list.some(x => x.id === assignedId)) {
          // add placeholder entry
          list.push({ id: assignedId, name: `Usuário ${assignedId}`, position: Number(posKey), phoneNumber: '', avatarUrl: '', available: null })
          membersByPosition.value[String(posKey)] = list
        }
      }
    }
  } catch (err) {
    Notify.create({ type: 'negative', message: 'Erro ao carregar dados.' })
  } finally {
    loading.value = false
  }
}

async function onSelect(scheduleId, position, newUserId) {
  const prev = assignments.value[scheduleId][position]
  if (prev === newUserId) return

  // find selected member
  const list = membersByPosition.value[position] || []
  const member = list.find(m => m.id === newUserId)

  if (member && member.available === false) {
    // confirm
    const confirmed = await $q.dialog({
      title: 'Confirmar escolha',
      message: 'Este membro está marcado como indisponível. Deseja mesmo atribuir?',
      cancel: true,
      persistent: true,
      ok: { label: 'Sim', color: 'primary' },
      cancel: { label: 'Cancelar' }
    }).onOk(() => true).onCancel(() => false).onDismiss(() => false)

    if (!confirmed) {
      // keep previous
      return
    }
  }

  // commit locally (save happens only when user clicks Salvar)
  assignments.value = { ...assignments.value, [scheduleId]: { ...assignments.value[scheduleId], [position]: newUserId } }
}

async function save() {
  saving.value = true
  try {
    const ids = schedules.value.map(s => s.scheduleId)
    for (const sid of ids) {
      const payload = { assignments: assignments.value[sid] || {} }
      await api.post(`schedules/${sid}/assignments`, payload)
    }
    Notify.create({ type: 'positive', message: 'Atribuições salvas.' })
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
    const ids = schedules.value.map(s => s.scheduleId)
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
</style>