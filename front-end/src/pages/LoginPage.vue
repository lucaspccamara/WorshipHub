<template>
  <div class="login-container">
    <q-card class="login-card" :class="{ 'keyboard-open': isKeyboardOpen }">
      <q-card-section>
        <div class="text-h6 text-center">Login</div>
  
        <q-input
          v-model="email"
          label="Email"
          type="email"
          dense
          outlined
          class="q-mt-md"
        />
  
        <q-input
          v-model="password"
          label="Senha"
          :type="isPwd ? 'password' : 'text'"
          dense
          outlined
          class="q-mt-md"
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
        <q-btn color="primary" label="Entrar" @click="login()" class="full-width q-mt-md" />
        <q-btn
          flat
          label="Esqueci minha senha"
          color="secondary"
          class="full-width q-mt-sm"
          @click="$router.push({ path: 'request-password-reset-code' })"
        />
      </q-card-actions>
    </q-card>
  </div>
</template>

<script setup>
import { ref } from "vue";
import api from "../api";
import { useRouter } from "vue-router";
import { useAuth } from "../composables/useAuth";
import { useKeyboardStatus } from '../composables/useKeyboardStatus'

const { isKeyboardOpen } = useKeyboardStatus()

const { setToken } = useAuth();
const router = useRouter();
const email = ref("");
const password = ref("");
const isPwd = ref(true);

const login = async () => {
  try {
    await api.getPost("auths/login", {
      email: email.value,
      password: password.value
    }).then(response => {
      if (response.status === 200 && response.data.token) {
        setToken(response.data.token);
        setTimeout(() => {
          router.push({ path: "/" });
        }, 200);
      } else {
        console.error("Token inválido ou não recebido.");
      }
    });

  } catch (error) {
    console.error("Erro ao fazer login:", error);
  }
};
</script>