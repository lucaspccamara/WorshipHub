<template>
  <q-card>
    <div class="card-header">
      <span class="text-h6 header-label">Alterar Senha</span>
      <q-btn class="float-right" dense flat icon="fa fa-close" v-close-popup>
        <q-tooltip>Fechar</q-tooltip>
      </q-btn>
    </div>

    <q-card class="q-pa-sm">
      <q-form @submit.prevent="submitPasswordChange" ref="formRef">
        <q-card-section class="q-gutter-md">
          <q-input
            v-model="form.currentPassword"
            :type="isPwd ? 'password' : 'text'"
            label="Senha atual"
            filled
            lazy-rules
            :rules="[val => !!val || 'Senha atual é obrigatória']"
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
          
          <q-input
            v-model="form.newPassword"
            :type="isPwd ? 'password' : 'text'"
            label="Nova senha"
            filled
            lazy-rules
            :rules="[
              val => !!val || 'Nova senha é obrigatória',
              val => val.length >= 6 || 'Senha deve ter ao menos 6 caracteres'
            ]"
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
          <q-input
            v-model="form.confirmPassword"
            :type="isPwd ? 'password' : 'text'"
            label="Confirmar nova senha"
            filled
            lazy-rules
            :rules="[
              val => !!val || 'Confirmação obrigatória',
              val => val === form.newPassword || 'As senhas não coincidem'
            ]"
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
        </q-card-section>
  
        <q-card-actions align="right">
          <q-btn flat label="Cancelar" color="primary" v-close-popup />
          <q-btn type="submit" label="Alterar" color="primary" />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-card>
</template>

<script setup>
import { ref } from 'vue'
import { Notify } from 'quasar'
import api from '../api'

const emit = defineEmits(['closeDialog']);
const props = defineProps({
  userEmail: {
    type: String,
    default: null
  }
})

const isPwd = ref(true);
const formRef = ref(null)
const form = ref({
  email: props.userEmail,
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})

async function submitPasswordChange() {
  const isValid = await formRef.value.validate()
  if (!isValid) return

  try {
    await api.post('auths/change-password', form.value).then(() => {
      Notify.create({ type: 'positive', message: 'Senha alterada com sucesso!' })
      form.value.email = ''
      form.value.currentPassword = ''
      form.value.newPassword = ''
      form.value.confirmPassword = ''
      formRef.value.resetValidation()
    }).finally(() => {
      emit('closeDialog')
    });
  } catch (err) {
    Notify.create({ type: 'negative', message: 'Erro ao alterar senha.' })
  }
}
</script>