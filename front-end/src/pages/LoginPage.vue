<template>
  <q-layout>
    <q-page-container>
      <q-page class="flex flex-center">
        <q-card class="login-card">
          <q-card-section>
            <div class="text-h6">Login</div>
            <q-input v-model="email" label="Email" />
            <q-input v-model="password" label="Password" type="password" />
            <q-btn color="primary" label="Login" @click="login" />
          </q-card-section>
        </q-card>
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<script>
import { ref } from 'vue';
import api from '../api';
import { Notify } from 'quasar';

export default {
  setup() {
    const email = ref('');
    const password = ref('');

    const login = () => {
      api.getPost('auths/login', {email: email.value, senha: password.value}).then((response) => {
        console.log(response);
      })
      Notify.create({
        message: 'Login successful',
        color: 'positive'
      });
    };

    return {
      email,
      password,
      login
    };
  }
};
</script>

<style>
.login-card {
  max-width: 400px;
  width: 90%;
}
</style>
