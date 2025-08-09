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
            maxlength="6"
            type="text"
            inputmode="numeric"
            pattern="[0-9]*"
            class="digit-input"
            input-class="text-center text-h6"
            @update:model-value="val => onDigitModelUpdate(val, index)"
            @paste="e => onPasteEvent(e, index)"
            @keyup="onKeyup($event, index)"
            @keydown.backspace="onBackspace(index)"
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
import { useKeyboardStatus } from '../composables/useKeyboardStatus';

const { isKeyboardOpen } = useKeyboardStatus()

const route = useRoute()
const router = useRouter();
const email = ref('');
const digits = ref(['', '', '', '', '', ''])
const digitInputs = ref([])
const lastValues = ref([...digits.value]);

// Flag para evitar reprocessar atualizações programáticas
let isProgrammatic = false;

onMounted(() => {
  email.value = route.query.email
  lastValues.value = [...digits.value]
  nextTick(() => digitInputs.value[0]?.focus())
})

function setLastValuesFromDigits() {
  lastValues.value = digits.value.slice();
}

/* ---- foco com controle de seleção ----
 * selectAll = true  -> seleciona todo o conteúdo (escrever substitui)
 * selectAll = false -> posiciona o cursor no fim (útil para backspace)
 */
function focusIndex(i, selectAll = true) {
  if (i < 0) i = 0;
  if (i >= digits.value.length) i = digits.value.length - 1;

  nextTick(() => {
    // digitInputs stores the QInput components; precisamos pegar o input real
    const comp = digitInputs.value[i];
    const el = comp?.$el?.querySelector('input') || (comp && comp.tagName === 'INPUT' ? comp : null);
    if (!el) return;
    el.focus();
    try {
      if (selectAll) {
        el.setSelectionRange(0, el.value.length);
      } else {
        el.setSelectionRange(el.value.length, el.value.length);
      }
    } catch {
      // alguns ambientes podem lançar aqui, ignore
    }
  });
}

/* ---- distribui uma string de dígitos a partir de um índice (autocomplete / paste longo) ---- */
function distributeFrom(index, onlyDigits) {
  isProgrammatic = true;
  const remaining = digits.value.length - index;
  const toApply = onlyDigits.slice(0, remaining).split('');
  for (let k = 0; k < toApply.length; k++) {
    digits.value[index + k] = toApply[k];
  }
  setLastValuesFromDigits();
  isProgrammatic = false;

  const next = index + toApply.length;
  focusIndex(next < digits.value.length ? next : digits.value.length - 1, true);
}

/* ----- paste event no campo ----- */
function onPasteEvent(e, index) {
  e.preventDefault();
  const text = (e.clipboardData?.getData('text') || '').replace(/\D/g, '').slice(0, 6);
  if (!text) return;

  // se colou só 1 dígito -> substituir o atual e avançar
  if (text.length === 1) {
    isProgrammatic = true;
    digits.value[index] = text;
    setLastValuesFromDigits();
    isProgrammatic = false;
    focusIndex(index + 1, true);
    return;
  }

  // colou vários -> distribui (autocomplete/paste)
  distributeFrom(index, text);
}

/* ----- principal: atualização vinda do update:model-value ----- */
function onDigitModelUpdate(value, index) {
  if (isProgrammatic) return; // ignorar alterações programáticas

  const only = String(value || '').replace(/\D/g, '');      // novo conteúdo bruto do input
  const prev = String(lastValues.value[index] || '').replace(/\D/g, ''); // snapshot do valor anterior

  // se limpou
  if (!only) {
    isProgrammatic = true;
    digits.value[index] = '';
    setLastValuesFromDigits();
    isProgrammatic = false;
    return;
  }

  const remainingSlots = digits.value.length - index;

  // caso: autocomplete/paste que preenche todas as células restantes -> distribui
  if (only.length >= remainingSlots) {
    distributeFrom(index, only);
    return;
  }

  // caso trivial: só 1 dígito no campo -> sobrescreve e avança 1
  if (only.length === 1) {
    isProgrammatic = true;
    digits.value[index] = only;
    setLastValuesFromDigits();
    isProgrammatic = false;
    focusIndex(index + 1, true);
    return;
  }

  // se chegou aqui: only.length > 1 mas não enche todas as células restantes
  // Se prev vazio -> provavelmente colagem curta em campo vazio -> distribui na sequência
  if (!prev) {
    isProgrammatic = true;
    for (let k = 0; k < only.length && (index + k) < digits.value.length; k++) {
      digits.value[index + k] = only[k];
    }
    setLastValuesFromDigits();
    isProgrammatic = false;
    focusIndex(index + only.length, true);
    return;
  }

  // caso: prev existe (edição sobre um campo já preenchido)
  // objetivo: identificar apenas o dígito novo e substituir apenas o índice atual
  // abordagem robusta e simples:
  // - procure o primeiro caractere em `only` que seja diferente de `prev` -> será o novo dígito
  // - se não encontrar, use o último caractere como fallback
  const chars = only.split('');
  let newDigit = chars.find(c => c !== prev) || chars[0] || prev;

  // aplica somente ao índice atual (NUNCA escrevemos os demais aqui)
  isProgrammatic = true;
  digits.value[index] = newDigit;
  setLastValuesFromDigits();
  isProgrammatic = false;

  // foca o próximo campo (apenas +1) e seleciona-o para facilitar substituição
  focusIndex(index + 1, true);
}

/* ----- backspace ----- */
function onBackspace(index) {
  // se já vazio e voltar
  if (!digits.value[index] && index > 0) {
    // foco no anterior e posicione o cursor no fim (apagar)
    focusIndex(index - 1, false);
  }
}

const submit = async () => {
  try {
    const code = digits.value.join('');
    if (code.length === 6) {
      await api.post('auths/verify-reset-code', { email: email.value, code: code }).then((response) => {
        if (response.status === 200) {
          Notify.create({ type: 'positive', message: 'Verificação concluída! Redefina sua senha.' });
          router.push({ path: 'reset-password' });
        }
      });
    } else {
      Notify.create({ type: 'negative', message: 'Código incompleto.' })
    }
  } catch {
    Notify.create({ type: 'negative', message: 'Erro na verificação.' })
  }
}
</script>

<style scoped>
.digit-input {
  width: 40px;
}
</style>