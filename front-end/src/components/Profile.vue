<template>
  <AppSectionHeader 
    :title="userIdProp == undefined ? 'Perfil do Usuário' : 'Editar Usuário'" 
    icon="fa fa-user" 
    :show-close="userIdProp !== undefined" 
  />

  <q-card class="q-pa-sm" :class="{ 'profile-scroll' : userIdProp !== undefined , 'profile-as-component' : userIdProp === undefined }">
    <q-form @submit.prevent="submitForm" ref="formRef">
      <q-card-section class="row q-col-gutter-md">
        <div class="col-12 col-md-6">
          <q-input
            v-model="form.name"
            label="Nome"
            filled
            lazy-rules
            :rules="[val => !!val || 'Nome é obrigatório']"
          />
        </div>
        <div class="col-12 col-md-6">
          <q-input
            v-model="form.phoneNumber"
            label="Celular"
            mask="(##) #####-####"
            filled
            lazy-rules
            :rules="[val => !!val || 'Celular é obrigatório']"
          />
        </div>
        <div class="col-12 col-md-6">
          <q-input
            v-model="form.email"
            label="E-mail"
            type="email"
            filled
            lazy-rules
            :rules="[
              val => !!val || 'E-mail é obrigatório',
              val => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(val) || 'E-mail inválido'
            ]"
          />
        </div>
        <div class="col-12 col-md-6">
          <q-select
            v-model="form.position"
            :options="PositionOptions"
            emit-value
            map-options
            label="Função"
            multiple
            use-chips
            filled
            popup-content-style="max-height: 200px;"
            :rules="[val => val.length > 0 || 'Ao menos uma função é obrigatória']"
          >
            <template v-slot:option="{ itemProps, opt, selected, toggleOption }">
              <q-item v-bind="itemProps">
                <q-item-section>
                  <q-item-label v-html="opt.label" />
                </q-item-section>
                <q-item-section side>
                  <q-toggle :model-value="selected" @update:model-value="toggleOption(opt)" />
                </q-item-section>
              </q-item>
            </template>
          </q-select>
        </div>

        <div class="col-12">
          <div class="text-subtitle1 q-mb-sm">Escolha seu Avatar</div>
          <div class="row q-col-gutter-sm">
            <div
              v-for="avatar in avatarOptions"
              :key="avatar"
              class="col-auto"
            >
              <q-img
                :src="avatar"
                :class="['avatar-option', { 'selected-avatar': form.avatarUrl === avatar }]"
                @click="form.avatarUrl = avatar"
                style="width: 64px; height: 64px; border-radius: 50%; cursor: pointer; border: 3px solid transparent"
              />
            </div>
          </div>
        </div>
        <div class="col-12 col-md-4" v-if="canEditRole">
          <q-select
            v-model="form.role"
            :options="RoleOptions"
            emit-value
            map-options
            label="Cargo"
            filled
            lazy-rules
            :rules="[val => val !== null || 'Cargo é obrigatório']"
          />
        </div>

        <!-- Timezone — visível apenas no perfil próprio (sem userIdProp) -->
        <div class="col-12 col-md-4" v-if="userIdProp === undefined">
          <q-select
            v-model="form.timezone"
            :options="timezoneOptions"
            emit-value
            map-options
            label="Fuso Horário"
            filled
            use-input
            input-debounce="0"
            @filter="filterTimezones"
            popup-content-style="max-height: 220px;"
          >
            <template v-slot:hint>
              Usado para enviar lembretes de escalas no seu horário local (12h)
            </template>
          </q-select>
        </div>
        
        <div class="col-12 col-md-4" v-if="canEditRole">
          <q-toggle
            v-model="form.status"
            :label="form.status ? 'Status: Ativo' : 'Status: Inativo'"
            color="primary"
          />
        </div>
      </q-card-section>
      <q-card-actions class="block" style="height: 50px;">
        <q-btn v-if="canChangePassword" class="float-left" label="Alterar Senha" color="primary" @click="dialogChangePassword = true" />
        <q-btn class="float-right" label="Salvar" color="primary" type="submit" />
      </q-card-actions>
    </q-form>
  </q-card>

  <q-dialog v-model="dialogChangePassword" persistent class="sm-dialog">
    <ChangePassword
      :userEmail="form.email"
      @closeDialog="dialogChangePassword = false"
    />
  </q-dialog>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { Notify } from 'quasar';
import AppSectionHeader from './AppSectionHeader.vue';
import api from '../api';
import { Role } from '../constants/Role';
import { RoleOptions } from '../constants/RoleOptions';
import { PositionOptions } from '../constants/PositionOptions';
import ChangePassword from './ChangePassword.vue';
import { useAuthStore } from '../stores/authStore';

const authStore = useAuthStore();

const canEditRole = computed(() => {
  return authStore.hasAnyRole([Role.Admin, Role.Leader]);
})

const canChangePassword = computed(() => {
  return authStore.isUserEmail(form.value.email);
})

const emit = defineEmits(['updateUsersList', 'closeDialog']);
const props = defineProps({
  userIdProp: {
    type: Number
  }
})

const userIdToLoad = ref(props.userIdProp || authStore.userId);

const defaultAvatar = '/avatars/default.png';
const avatarOptions = [
  '/avatars/1.png',
  '/avatars/2.png',
  '/avatars/3.png',
  '/avatars/4.png',
  '/avatars/5.png',
  '/avatars/6.png',
  '/avatars/7.png',
  '/avatars/8.png'
];

const formRef = ref(null)
const form = ref({
  id: 0,
  name: '',
  phoneNumber: '',
  email: '',
  position: [],
  avatarUrl: '',
  role: 2,
  status: true,
  timezone: 'America/Sao_Paulo'
})

const dialogChangePassword = ref(false);

// Common IANA timezones — label shown to user, value sent to API
const allTimezones = [
  { label: 'Brasília (BRT, -03:00)', value: 'America/Sao_Paulo' },
  { label: 'Manaus (AMT, -04:00)', value: 'America/Manaus' },
  { label: 'Rio Branco (ACT, -05:00)', value: 'America/Rio_Branco' },
  { label: 'Fernando de Noronha (FNT, -02:00)', value: 'America/Noronha' },
  { label: 'Buenos Aires (ART, -03:00)', value: 'America/Argentina/Buenos_Aires' },
  { label: 'Lisboa (WET/WEST, +00:00)', value: 'Europe/Lisbon' },
  { label: 'Londres (GMT/BST, +00:00)', value: 'Europe/London' },
  { label: 'Nova Iorque (EST/EDT, -05:00)', value: 'America/New_York' },
  { label: 'Los Angeles (PST/PDT, -08:00)', value: 'America/Los_Angeles' },
  { label: 'UTC', value: 'UTC' },
];

const timezoneOptions = ref(allTimezones);

function filterTimezones(val, update) {
  update(() => {
    const q = val.toLowerCase();
    timezoneOptions.value = allTimezones.filter(tz =>
      tz.label.toLowerCase().includes(q) || tz.value.toLowerCase().includes(q)
    );
  });
}

async function loadUserProfile() {
  try {
    const response = await api.get('users/profile', userIdToLoad.value);
    const user = response.data;

    Object.assign(form.value, {
      id: user.id,
      name: user.name,
      phoneNumber: user.phoneNumber,
      email: user.email,
      position: user.position || [],
      avatarUrl: user.avatarUrl || defaultAvatar,
      role: user.role,
      status: user.status,
      timezone: user.timezone || 'America/Sao_Paulo'
    });

    if (!avatarOptions.includes(form.value.avatarUrl)) {
      avatarOptions.push(form.value.avatarUrl);
    }
  } catch (error) {
    Notify.create({
      type: 'negative',
      message: 'Erro ao carregar perfil do usuário.'
    });
  }
}

async function submitForm() {
  const isValid = await formRef.value.validate()
  if (!isValid) return

  try {
    await api.put('users/profile', form.value.id, form.value);

    // Se é o próprio perfil, também atualiza o timezone separadamente
    if (props.userIdProp === undefined) {
      await api.patch(`users/${form.value.id}/timezone`, null, { timezone: form.value.timezone });
    }

    Notify.create({ type: 'positive', message: 'Perfil atualizado com sucesso!' });
    formRef.value.resetValidation();
    emit('updateUsersList');
    emit('closeDialog');
  } catch (err) {
    Notify.create({ type: 'negative', message: 'Erro ao atualizar perfil.' })
  }
}

onMounted(async () => {
  loadUserProfile();
})
</script>

<style scoped>
.selected-avatar {
  border-color: #1976d2 !important;
  box-shadow: 0 0 5px rgba(25, 118, 210, 0.6);
}

.lg-dialog .profile-scroll {
  max-height: 485px;
  overflow-y: auto;
}

.profile-as-component {
  background: #f1f2f3;
  box-shadow: none;
}
</style>