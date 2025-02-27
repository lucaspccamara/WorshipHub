<template>
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
</template>

<script setup>
import { ref } from "vue";
import api from "../api";
import { Cookies } from "quasar";
import { useRouter } from "vue-router";

const router = useRouter();
const email = ref("");
const password = ref("");
const isPwd = ref(true);

const login = async () => {
  try {
    const response = await api.getPost("auths/login", {
      email: email.value,
      password: password.value
    });

    if (response.status === 200) {
      Cookies.set("user_token", response.data.token, { expires: "6h" });
      router.push({ path: "/" });
    }
  } catch (error) {
    console.error("Erro ao fazer login:", error);
  }
};
</script>

<style lang="scss">
.login-card {
  align-self: center;
  width: 90%;
  max-width: 400px;
  padding: 16px;
  max-height: 90vh;
  margin-bottom: 20px;
}
</style>