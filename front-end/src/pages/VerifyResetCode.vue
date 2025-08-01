<template>
  <div class="login-container">
    <q-card class="login-card" :class="{ 'keyboard-open': isKeyboardOpen }">
      <q-card-section>
        <div class="text-h6">Insira o código de verificação</div>
      </q-card-section>
      <q-form @submit.prevent="submit">
        <q-card-section class="row justify-center q-gutter-sm">
          <q-input
            v-for="(digit, index) in digits"
            :key="index"
            v-model="digits[index]"
            ref="digitInputs"
            maxlength="1"
            type="text"
            inputmode="numeric"
            pattern="[0-9]*"
            mask="#"
            class="digit-input"
            input-class="text-center text-h6"
            @keyup="onKeyup($event, index)"
            @keydown.backspace="onBackspace($event, index)"
            @focus="onFocus(index)"
          />
        </q-card-section>
        <q-card-actions>
          <q-btn type="submit" label="Verificar" color="primary" class="full-width q-mt-md"/>
        </q-card-actions>
      </q-form>
    </q-card>
  </div>
</template>

<script setup>
import { ref, onMounted, nextTick } from 'vue';
import api from '../api';
import { useRoute, useRouter } from "vue-router";
import { Notify } from 'quasar';
import { useKeyboardStatus } from '../composables/useKeyboardStatus'

const { isKeyboardOpen } = useKeyboardStatus()

const route = useRoute()
const router = useRouter();
const email = ref('');
const digits = ref(['', '', '', '', '', '']);
const digitInputs = ref([]);

const onKeyup = (event, index) => {
  const value = event.target.value.replace(/\D/g, '');

  if (value.length > 1) {
    digits.value[index] = value.slice(0, 1);
  } else {
    digits.value[index] = value;
  }

  if (value && index < 5) {
    nextTick(() => digitInputs.value[index + 1]?.focus());
  }
};

const onBackspace = (event, index) => {
  if (!digits.value[index] && index > 0) {
    nextTick(() => digitInputs.value[index - 1]?.focus());
  }
};

const onFocus = (index) => {
  // Seleciona o valor atual para facilitar reedição
  nextTick(() => {
    const el = digitInputs.value[index]?.$el.querySelector('input');
    el?.select();
  });
};

const submit = async () => {
  try {
    const code = digits.value.join('');
    if (code.length === 6) {
      await api.post('auths/verify-reset-code', { email: email.value, code: verficationCode.value }).then((response) => {
        if (response.status === 200) {
          Notify.create({ type: 'positive', message: 'Verificação concluída! Redefina sua senha.' });
          router.push({ path: 'reset-password', query: { token: response.data } });
        }
      });
    } else {
      Notify.create({ type: 'negative', message: 'Código incompleto.' })
    }
  } catch {
    Notify.create({ type: 'negative', message: 'Erro na verificação.' })
  }
}

onMounted(() => {
  email.value = route.query.email
  nextTick(() => digitInputs.value[0]?.focus());
})
</script>

<style scoped>
.digit-input {
  width: 40px;
}
</style>