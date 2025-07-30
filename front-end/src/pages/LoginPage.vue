<template>
  <div class="login-container">
    <q-card class="login-card">
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
      </q-card-actions>
    </q-card>
  </div>
</template>

<script setup>
import { ref } from "vue";
import api from "../api";
import { useRouter } from "vue-router";
import { useAuth } from "../composables/useAuth";

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

<style lang="scss">
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  height: calc(100vh - 56px);
  width: 100%;
}

.login-card {
  width: 90%;
  max-width: 500px;
  padding: 16px;
}
</style>