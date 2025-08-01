<template>
  <div class="login-container">
    <q-card class="login-card" :class="{ 'keyboard-open': isKeyboardOpen }">
      <q-card-section>
        <div class="text-h6">Recuperar senha</div>
      </q-card-section>
      <q-form @submit.prevent="submit">
        <q-card-section>
          <q-input v-model="email" type="email" label="E-mail" required />
        </q-card-section>
        <q-card-actions>
          <q-btn type="submit" label="Enviar" color="primary" class="full-width q-mt-md"/>
        </q-card-actions>
      </q-form>
    </q-card>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import api from '../api';
import { useRouter } from "vue-router";
import { Notify } from 'quasar';
import { useKeyboardStatus } from '../composables/useKeyboardStatus'

const { isKeyboardOpen } = useKeyboardStatus()

const router = useRouter();
const email = ref('');

const submit = async () => {
  try {
    await api.post('auths/request-password-reset-code', { email: email.value }).then((response) => {
      if (response.status === 200) {
        Notify.create({ type: 'positive', message: 'Um e-mail com o código de verficiação foi enviado com sucesso.' });
        router.push({ path: 'verify-reset-code', query: { email: email.value } });
      }
    });
  } catch {
    Notify.create({ type: 'negative', message: 'Erro ao solicitar redefinição.' })
  }
}
</script>