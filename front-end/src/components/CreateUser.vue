<template>
  <q-card>
    <div class="card-header">
      <span class="text-h6 header-label">Cadastrar Usuário</span>
      <q-btn class="float-right" dense flat icon="fa fa-close" v-close-popup>
        <q-tooltip>Fechar</q-tooltip>
      </q-btn>
    </div>
    
    <q-card class="q-pa-sm">
      <q-form @submit.prevent="submitForm" ref="formRef">
        <q-card-section class="row q-col-gutter-md">
          <div class="col-12">
            <q-input
              filled
              v-model="form.name"
              label="Nome"
              lazy-rules
              :rules="[val => !!val || 'Nome é obrigatório']"
            />
          </div>
          <div class="col-12">
            <q-input
              filled
              v-model="form.email"
              label="E-mail"
              type="email"
              lazy-rules
              :rules="[
                val => !!val || 'E-mail é obrigatório',
                val => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(val) || 'E-mail inválido'
              ]"
            />
          </div>
          <div class="col-12">
            <q-select
              v-model="form.position"
              :options="PositionOptions"
              emit-value
              map-options
              label="Função"
              multiple
              use-chips
              filled
              popup-content-style="max-height: 200px;"
              :rules="[val => val.length > 0 || 'Ao menos uma função é obrigatória']"
            >
              <template v-slot:option="{ itemProps, opt, selected, toggleOption }">
                <q-item v-bind="itemProps">
                  <q-item-section>
                    <q-item-label v-html="opt.label" />
                  </q-item-section>
                  <q-item-section side>
                    <q-toggle :model-value="selected" @update:model-value="toggleOption(opt)" />
                  </q-item-section>
                </q-item>
              </template>
            </q-select>
          </div>
          <div class="col-12">
            <q-input
              filled
              v-model="form.password"
              label="Senha"
              :type="isPwd ? 'password' : 'text'"
              lazy-rules
              :rules="[val => !!val || 'Senha é obrigatória']"
            >
              <template v-slot:append>
                <q-icon
                  :name="isPwd ? 'fa-solid fa-eye-slash' : 'fa-solid fa-eye'"
                  class="cursor-pointer"
                  @click="isPwd = !isPwd"
                  size="xs"
                />
              </template>
            </q-input>
          </div>
        </q-card-section>
  
        <q-card-actions align="right">
          <q-btn label="Cadastrar" color="primary" type="submit" />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-card>
</template>

<script setup>
import { ref } from 'vue';
import { Notify } from 'quasar';
import api from '../api';
import { PositionOptions } from '../constants/PositionOptions';

const emit = defineEmits(['updateUsersList', 'closeDialog']);

const isPwd = ref(true);
const form = ref({
  name: '',
  email: '',
  position: [],
  password: ''
})

const formRef = ref(null)

async function submitForm() {
  const isValid = await formRef.value.validate()
  if (!isValid) return

  try {
    await api.post('users', form.value).then(() => {
      Notify.create({ type: 'positive', message: 'Usuário cadastrado com sucesso!' })
      form.value.name = ''
      form.value.email = ''
      form.value.position = []
      form.value.password = ''
      formRef.value.resetValidation()
    }).finally(() => {
      emit('updateUsersList');
      emit('closeDialog');
    });
  } catch (err) {
    Notify.create({ type: 'negative', message: 'Erro ao cadastrar usuário.' })
  }
}
</script>