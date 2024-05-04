<template>
  <q-layout>
    <q-page-container>
      <q-page class="flex flex-center">
        <q-card class="login-card">
          <q-card-section>
            <div class="text-h6">Login</div>
            <q-input v-model="email" label="Email" />
            <q-input v-model="password" label="Password" type="password" />
            <q-btn color="primary" label="Login" @click="login()" />
          </q-card-section>
        </q-card>
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<script>
import { ref } from 'vue'
import api from '../api'
import { Cookies } from 'quasar'
import { useRouter } from 'vue-router'

export default {
  setup() {
    const router = useRouter();
    const email = ref('');
    const password = ref('');

    function login() {
      api.getPost('auths/login', {email: email.value, senha: password.value}).then((response) => {
        if (response.status === 200) {
          Cookies.set('user_token', response.data.token, { expires: '6h' })
          router.push({ path: '/' })
        }
      })
    };

    return {
      email,
      password,
      login
    };
  }
};
</script>

<style lang="scss">
.login-card {
  max-width: 400px;
  width: 90%;
}
</style>
