<template>
  <div class="login-container">
    <q-card class="login-card" :class="{ 'keyboard-open': isKeyboardOpen }">
      <q-card-section>
        <div class="text-h6">Redefinir Senha</div>
      </q-card-section>

      <q-card-section>
        <q-input
          v-model="newPassword"
          :type="isPwd ? 'password' : 'text'"
          label="Nova Senha"
          filled
          lazy-rules
          :rules="[val => !!val || 'Senha obrigatória']"
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
          v-model="confirmPassword"
          :type="isPwd ? 'password' : 'text'"
          label="Confirmar Nova Senha"
          filled
          lazy-rules
          :rules="[val => !!val || 'Confirmação obrigatória', val => val === newPassword || 'As senhas não coincidem']"
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

      <q-card-actions>
        <q-btn color="primary" label="Redefinir Senha" @click="submit" class="full-width q-mt-md" />
      </q-card-actions>
    </q-card>
  </div>
</template>

<script setup>
import { ref} from 'vue';
import { useRouter } from 'vue-router';
import api from '../api';
import { Notify } from 'quasar';
import { useKeyboardStatus } from '../composables/useKeyboardStatus';

const { isKeyboardOpen } = useKeyboardStatus()

const router = useRouter()
const newPassword = ref('')
const confirmPassword = ref('')
const isPwd = ref(true);

const submit = async () => {
  if (newPassword.value !== confirmPassword.value) {
    Notify.create({ type: 'negative', message: 'As senhas não coincidem.' })
    return
  }

  try {
    await api.post('auths/reset-password', {
      newPassword: newPassword.value
    }).then(() => {
      // Redireciona para a página de login após redefinir a senha
      Notify.create({ type: 'positive', message: 'Senha redefinida com sucesso.' })
      setTimeout(() => {
        router.push({ path: '/login' })
      }, 200)
    })
  } catch {
    Notify.create({ type: 'negative', message: 'Token inválido ou expirado.' })
    setTimeout(() => {
      router.push({ path: '/login' })
    }, 200)
  }
}
</script>