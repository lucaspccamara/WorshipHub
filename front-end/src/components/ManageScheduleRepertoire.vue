<template>
  <q-card class="column full-height bg-white">
    <div v-if="!hideHeader" class="col-auto row items-center q-pa-sm bg-white text-dark" style="border-bottom: 1px solid var(--q-separator)">
      <div class="text-subtitle1 text-weight-bold q-ml-sm">Repertório</div>
      <q-space />
      <q-btn dense flat icon="fa fa-close" v-close-popup />
    </div>

    <q-card-section class="col overflow-auto">
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
          <div class="row items-center justify-between q-mb-sm">
            <div class="text-subtitle2">Músicas do Repertório</div>
            <q-btn color="primary" icon="fa fa-plus" label="Adicionar Música" size="sm" @click="dialogMusicList = true" />
          </div>

          <q-list bordered separator class="rounded-borders">
            <q-item v-for="(track, index) in schedule.repertoire" :key="index">
              <q-item-section>
                <q-item-label>{{ track.title }} <span class="text-caption text-grey" v-if="track.artist">— {{ track.artist }}</span></q-item-label>
                <q-item-label caption>
                  Tom: <strong>{{ formatKey(track.noteBase, track.noteMode) }}</strong> | BPM: <strong>{{ track.bpm || '--' }}</strong>
                </q-item-label>
              </q-item-section>
              <q-item-section side>
                <div class="row q-gutter-xs">
                  <q-btn flat dense round icon="fa fa-cog" color="primary" @click="openConfigTrack(index)" />
                  <q-btn flat dense round icon="fa fa-trash" color="negative" @click="removeTrack(index)" />
                </div>
              </q-item-section>
            </q-item>
            <q-item v-if="schedule.repertoire.length === 0">
              <q-item-section class="text-grey text-center q-pa-md">Nenhuma música adicionada</q-item-section>
            </q-item>
          </q-list>
        </div>
      </div>
    </q-card-section>

    <q-separator v-if="!hideFooter" />

    <q-card-actions v-if="!hideFooter" align="right" class="col-auto bg-white q-pa-md">
      <q-btn color="primary" label="Salvar" @click="save" :loading="saving" />
      <q-btn v-if="showTransition" color="secondary" label="Salvar e Avançar" @click="saveAndAdvance" :loading="savingAdvance" />
    </q-card-actions>

    <!-- Modal for Music List -->
    <q-dialog v-model="dialogMusicList" maximized transition-show="slide-up" transition-hide="slide-down">
      <q-card>
        <q-bar class="card-header">
          <div class="text-h6">Selecionar Música</div>
          <q-space />
          <q-btn dense flat icon="fa fa-close" v-close-popup />
        </q-bar>
        <q-card-section class="q-pa-none">
          <MusicList selectable @selected="onMusicSelected" />
        </q-card-section>
      </q-card>
    </q-dialog>

    <!-- Modal for Config Track -->
    <q-dialog v-model="dialogConfigTrack">
      <q-card style="min-width: 300px">
        <q-card-section>
          <div class="text-h6">Configurar Música</div>
          <div class="text-subtitle2">{{ editingTrack?.title }} <span class="text-caption text-grey" v-if="editingTrack?.artist">— {{ editingTrack?.artist }}</span></div>
        </q-card-section>

        <q-card-section>
          <div class="row q-col-gutter-md">
            <div class="col-12">
              <q-select
                v-model="editNoteBase"
                :options="noteOptions"
                label="Tom (Nota)"
                outlined
                dense
                emit-value
                map-options
              />
            </div>
          </div>
          <div class="row q-mt-md">
            <div class="col-12">
              <q-input
                v-model.number="editBpm"
                label="BPM"
                type="number"
                outlined
                dense
              />
            </div>
          </div>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat label="Cancelar" color="primary" v-close-popup />
          <q-btn flat label="Confirmar" color="primary" @click="confirmConfigTrack" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-card>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { Notify } from 'quasar'
import api from '../api'
import { PositionOptions } from '../constants/PositionOptions'
import MusicList from './MusicList.vue'

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

const dialogMusicList = ref(false)
const dialogConfigTrack = ref(false)
const editingTrackIndex = ref(-1)
const editingTrack = ref(null)

const editNoteBase = ref('')
const editNoteMode = ref('');
const editBpm = ref('');

const noteOptions = computed(() => {
  const baseNotes = ['C','C#','D','Eb','E','F','F#','G','Ab','A','Bb','B'];
  const isMinor = editNoteMode.value === 'minor';
  return baseNotes.map(n => ({
    label: n + (isMinor ? 'm' : ''),
    value: n
  }));
});

function formatDate(d) {
  try { return new Date(d).toLocaleDateString(); } catch { return d }
}
function positionLabel(value) {
  const p = PositionOptions.find(x => x.value === Number(value))
  return p ? p.label : String(value)
}
function formatKey(base, mode) {
  if (!base) return '--'
  const isMinor = mode === 'minor' ? 'm' : ''
  return `${base}${isMinor}`
}

async function load() {
  loading.value = true
  try {
    const r = await api.get(`schedules/${props.scheduleId}/repertoire`);
    const dto = r.data
    schedule.value = {
      date: dto.date ?? dto.Date,
      assignedMembers: (dto.assignedMembers || dto.AssignedMembers || []).map(m => ({ id: m.id ?? m.Id, name: m.name ?? m.Name, position: m.position ?? m.Position })),
      repertoire: (dto.repertoire || dto.Repertoire || []).map(t => ({ 
        id: t.id ?? t.Id, 
        title: t.title ?? t.Title, 
        artist: t.artist ?? t.Artist,
        noteBase: t.noteBase ?? t.NoteBase,
        noteMode: t.noteMode ?? t.NoteMode,
        bpm: t.bpm ?? t.Bpm
      }))
    }
  } catch (err) {
    console.error(err)
    Notify.create({ type: 'negative', message: 'Erro ao carregar repertório.' })
  } finally {
    loading.value = false
  }
}

function onMusicSelected(music) {
  dialogMusicList.value = false;
  editingTrackIndex.value = -1;
  editingTrack.value = music;
  editNoteBase.value = music.noteBase || music.NoteBase || '';
  // Se vier vazio ou diferente de minor, setamos major como padrão visual ou mantemos
  const mode = (music.noteMode || music.NoteMode || 'major').toLowerCase();
  editNoteMode.value = mode === 'minor' ? 'minor' : 'major';
  editBpm.value = music.bpm || music.Bpm || '';
  dialogConfigTrack.value = true;
}

function openConfigTrack(index) {
  editingTrackIndex.value = index;
  const track = schedule.value.repertoire[index];
  editingTrack.value = track;
  editNoteBase.value = track.noteBase || '';
  const mode = (track.noteMode || track.NoteMode || 'major').toLowerCase();
  editNoteMode.value = mode === 'minor' ? 'minor' : 'major';
  editBpm.value = track.bpm || '';
  dialogConfigTrack.value = true;
}

function confirmConfigTrack() {
  const configuredTrack = {
    ...editingTrack.value,
    id: editingTrack.value.id || editingTrack.value.Id,
    title: editingTrack.value.title || editingTrack.value.Title,
    artist: editingTrack.value.artist || editingTrack.value.Artist,
    noteBase: editNoteBase.value,
    bpm: editBpm.value ? Number(editBpm.value) : null // Ignoramos gravar editNoteMode, herda o original
  };

  if (editingTrackIndex.value >= 0) {
    schedule.value.repertoire[editingTrackIndex.value] = configuredTrack;
  } else {
    schedule.value.repertoire.push(configuredTrack);
  }
  dialogConfigTrack.value = false;
}

function removeTrack(index) {
  schedule.value.repertoire.splice(index, 1);
}

async function save() {
  saving.value = true
  try {
    const payload = schedule.value.repertoire.map(t => ({
      musicId: t.id,
      noteBase: t.noteBase,
      noteMode: t.noteMode,
      bpm: t.bpm
    }));
    await api.post(`schedules/${props.scheduleId}/repertoire`, payload);
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