<template>
  <q-card>
    <AppSectionHeader 
      :title="musicId == 0 ? 'Cadastrar Música' : 'Editar Música'" 
      icon="fa-solid fa-music" 
      show-close 
    />
    
    <q-card class="q-pa-sm">
      <q-form @submit.prevent="submitForm" ref="formRef">
        <q-card-section class="row q-col-gutter-md">
          <div class="col-12" v-if="form.imageUrl">
            <q-img
              class="music-bg"
              :src="form.imageUrl"
              spinner-color="blue"
              fit="cover"
            />
          </div>

          <div class="col-12 col-md-4">
            <q-input
              label="Título"
              v-model="form.title"
              lazy-rules
              :rules="[val => !!val || 'Título é obrigatório']"
              filled
            />
          </div>

          <div class="col-12 col-md-4">
            <q-input
              label="Artista"
              v-model="form.artist"
              filled
            />
          </div>

          <div class="col-12 col-md-4">
            <q-input
              label="Álbum"
              v-model="form.album"
              filled
            />
          </div>
          
          <div class="col-12 col-md-2">
            <q-btn-group
              filled
              spread
            >
              <q-select
                label="Tom (original)"
                v-model="form.noteBase"
                :options="NoteBaseOptions"
                emit-value
                map-options
                filled
                class="col-7"
              />
              <q-select
                label="Modo"
                v-model="form.noteMode"
                :options="NoteModeOptions"
                emit-value
                map-options
                filled
                class="col"
              />
            </q-btn-group>
          </div>

          <div class="col-5 col-md-2">
            <q-input
              label="BPM"
              v-model.number="form.bpm"
              type="number"
              step="0.01"
              filled
              :max="999"
            />
          </div>

          <div class="col-7 col-md-2">
            <q-select
              label="Tempo (compasso)"
              v-model="form.timeSignature"
              :options="TimeSignatureOptions"
              emit-value
              map-options
              filled
            />
          </div>

          <div class="col-12 col-md-2">
            <q-input
              label="Duração"
              v-model="form.duration"
              type="time"
              mask="##:##"
              filled
            />
          </div>

          <div class="col-12 col-md-4">
            <q-input
              label="URL do Vídeo"
              v-model="form.videoUrl"
              filled
            />
          </div>

          <div class="col-12 col-md-4">
            <q-input
              label="URL da Imagem"
              v-model="form.imageUrl"
              filled
            />
          </div>
        </q-card-section>
  
        <q-card-actions align="right">
          <q-btn :label="musicId == 0 ? 'Cadastrar' : 'Salvar'" color="primary" type="submit" />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-card>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { Notify } from 'quasar';
import AppSectionHeader from './AppSectionHeader.vue';
import api from '../api';
import { NoteBaseOptions, NoteModeOptions } from '../constants/NoteOptions';
import { TimeSignatureOptions } from '../constants/TimeSignatureOptions';

const emit = defineEmits(['updateMusicList', 'closeDialog']);
const props = defineProps({
  musicId: {
    type: Number
  }
})

const form = ref({
  id: 0,
  title: '',
  artist: '',
  album: '',
  noteBase: '',
  noteMode: '',
  bpm: null,
  timeSignature: '',
  duration: null,
  videoUrl: ''
})

const formRef = ref(null)

async function loadMusic() {
  try {
    const response = await api.get('musics', props.musicId);
    const music = response.data;

    Object.assign(form.value, {
      id: music.id,
      title: music.title,
      artist: music.artist,
      album: music.album,
      noteBase: music.noteBase,
      noteMode: music.noteMode,
      bpm: music.bpm,
      timeSignature: music.timeSignature,
      duration: music.duration,
      videoUrl: music.videoUrl,
      imageUrl: music.imageUrl
    });
  } catch (error) {
    Notify.create({
      type: 'negative',
      message: 'Erro ao carregar música.'
    });
  }
}

async function submitForm() {
  const isValid = await formRef.value.validate()
  if (!isValid) return

  try {
    if (props.musicId == 0) {
      await api.post('musics', form.value).then(() => {
        Notify.create({ type: 'positive', message: 'Música cadastrada com sucesso!' })
        form.value.id = 0
        form.value.title = ''
        form.value.artist = ''
        form.value.album = ''
        form.value.noteBase = ''
        form.value.noteMode = ''
        form.value.bpm = null
        form.value.timeSignature = ''
        form.value.duration = null
        form.value.videoUrl = ''
        form.value.imageUrl = ''
        formRef.value.resetValidation()
      }).finally(() => {
        emit('updateMusicsList');
        emit('closeDialog');
      });
    } else {
      await api.put('musics', props.musicId, form.value).then(() => {
        Notify.create({ type: 'positive', message: 'Música atualizada com sucesso!' })
        form.value.id = 0
        form.value.title = ''
        form.value.artist = ''
        form.value.album = ''
        form.value.noteBase = ''
        form.value.noteMode = ''
        form.value.bpm = null
        form.value.timeSignature = ''
        form.value.duration = null
        form.value.videoUrl = ''
        form.value.imageUrl = ''
        formRef.value.resetValidation()
      }).finally(() => {
        emit('updateMusicList');
        emit('closeDialog');
      });
    }
  } catch (err) {
    Notify.create({ type: 'negative', message: 'Erro ao cadastrar música.' })
  }
}

onMounted(async () => {
  if (props.musicId > 0)
    loadMusic();
})
</script>

<style lang="scss">
.music-bg {
    position: absolute;
    inset: 0;
    filter: brightness(0.6);
    
    /* Aplicando fade diretamente na imagem */
    mask-image: linear-gradient(to bottom, rgba(0, 0, 0, 0.6) 0%, rgba(0, 0, 0, 0.4) 20%, rgba(0, 0, 0, 0.2) 50%, rgba(0, 0, 0, 0.05) 80%);
    -webkit-mask-image: linear-gradient(to bottom, rgba(0, 0, 0, 0.6) 0%, rgba(0, 0, 0, 0.4) 20%, rgba(0, 0, 0, 0.2) 50%, rgba(0, 0, 0, 0.05) 80%);
  }
</style>