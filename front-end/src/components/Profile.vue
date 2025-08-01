<template>
  <div class="card-header" v-if="userIdProp == undefined">
    <span class="text-h6 header-label">Perfil do Usuário</span>
  </div>
  <div class="card-header" v-else>
    <div class="text-h6 header-label">Editar Usuário</div>
    <q-btn class="float-right" dense flat icon="fa fa-close" v-close-popup>
      <q-tooltip>Fechar</q-tooltip>
    </q-btn>
  </div>

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
        <div class="col-12 col-md-6" v-if="canEditRole">
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
        <div class="col-12 col-md-6" v-if="canEditRole">
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
import api from '../api';
import { Role } from '../constants/Role';
import { RoleOptions } from '../constants/RoleOptions';
import { PositionOptions } from '../constants/PositionOptions';
import ChangePassword from './ChangePassword.vue';
import { useAuth } from '../composables/useAuth';

const { userId, hasAnyRole, isUserEmail } = useAuth();

const canEditRole = computed(() => {
  return hasAnyRole([Role.Admin, Role.Leader]);
})

const canChangePassword = computed(() => {
  return isUserEmail(form.value.email);
})

const emit = defineEmits(['updateUsersList', 'closeDialog']);
const props = defineProps({
  userIdProp: {
    type: Number
  }
})

const userIdToLoad = ref(props.userIdProp || userId.value);

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
  status: true
})

const dialogChangePassword = ref(false);

async function loadUserProfile() {
  try {
    const response = await api.getOne('users/profile', userIdToLoad.value);
    const user = response.data;

    Object.assign(form.value, {
      id: user.id,
      name: user.name,
      phoneNumber: user.phoneNumber,
      email: user.email,
      position: user.position || [],
      avatarUrl: user.avatarUrl || defaultAvatar,
      role: user.role,
      status: user.status
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
    await api.put('users/profile', form.value.id, form.value).then(() => {
      Notify.create({ type: 'positive', message: 'Perfil atualizado com sucesso!' })
      formRef.value.resetValidation();
    }).finally(() => {
      emit('updateUsersList');
      emit('closeDialog');
    });
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