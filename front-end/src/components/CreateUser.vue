<template>
  <q-card style="max-width: 500px">
    <q-bar class="card-header">
      <span>Cadastrar de Usuário</span>
      <q-space />
      <q-btn dense flat icon="fa fa-close" v-close-popup>
        <q-tooltip>Fechar</q-tooltip>
      </q-btn>
    </q-bar>
    
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
          <q-input
            filled
            v-model="form.password"
            label="Senha"
            type="password"
            lazy-rules
            :rules="[val => !!val || 'Senha é obrigatória']"
          />
        </div>
      </q-card-section>

      <q-card-actions align="right">
        <q-btn label="Cadastrar" color="primary" type="submit" />
      </q-card-actions>
    </q-form>
  </q-card>
</template>

<script setup>
import { ref } from 'vue';
import { Notify } from 'quasar';
import api from '../api';

const emit = defineEmits(['updateUsersList', 'closeDialog']);

const form = ref({
  name: '',
  email: '',
  password: ''
})

const formRef = ref(null)

async function submitForm() {
  const isValid = await formRef.value.validate()
  if (!isValid) return

  try {
    await api.create('users', form.value).then(() => {
      Notify.create({ type: 'positive', message: 'Usuário cadastrado com sucesso!' })
      form.value.name = ''
      form.value.email = ''
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