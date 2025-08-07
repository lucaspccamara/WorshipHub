<template>
  <q-header elevated class="bg-primary text-white">
    <q-toolbar>
      <!-- Botão do menu (mostrado apenas quando não está na tela de login) -->
      <q-btn
        v-if="showMenu"
        dense
        flat
        round
        icon="fa-solid fa-bars"
        @click="toggleLeftDrawer"
      />

      <!-- Logo e título -->
      <q-btn no-caps unelevated :ripple="false" to="/">
        <q-toolbar-title class="row items-center">
          <q-avatar size="32px">
            <img src="https://cdn.quasar.dev/logo-v2/svg/logo-mono-white.svg" alt="Logo" />
          </q-avatar>
          <span class="q-ml-sm text-subtitle1">WorshipHub</span>
        </q-toolbar-title>
      </q-btn>
    </q-toolbar>
  </q-header>

  <!-- Menu lateral -->
  <q-drawer v-model="drawer" side="left" behavior="mobile" elevated show-if-above>
    <q-list>
      <q-item
        v-for="(item, index) in visibleMenuItems"
        :key="index"
        clickable
        v-ripple
        @click="goTo(item.route)"
      >
        <q-item-section avatar>
          <q-icon :name="item.icon" />
        </q-item-section>
        <q-item-section>
          <q-item-label>{{ item.label }}</q-item-label>
        </q-item-section>
      </q-item>
    </q-list>
  </q-drawer>

  <q-dialog
    v-model="logoutDialog"
    persistent
    backdrop-filter="grayscale(100%)"
  >
    <q-card>
      <q-card-section class="row items-center">
        <div class="text-h6 header-label col-12">Deseja realmente sair?</div>
        <div class="text-body">Você será desconectado.</div>
      </q-card-section>
      
      <q-card-actions align="right">
        <q-btn flat label="Cancelar" v-close-popup />
        <q-btn label="Ok" color="primary" @click="logout()" />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref, computed } from "vue";
import { useRouter, useRoute } from "vue-router";
import api from '../api';
import { Role } from "../constants/Role";
import { useAuthStore } from "../stores/authStore";

const authStore = useAuthStore();

const drawer = ref(false);
const logoutDialog = ref(false);
const router = useRouter();
const route = useRoute();
const showMenu = computed(() => route.path !== "/login" && route.path !== "/request-password-reset-code" && route.path !== "/verify-reset-code" && route.path !== "/reset-password");

const menuItems = [
  { label: "Escalas", route: "/schedule", icon: "fa-solid fa-calendar", roles: [Role.Admin, Role.Leader, Role.Member, Role.Minister] },
  { label: "Músicas", route: "/songs", icon: "fa-solid fa-music", roles: [Role.Admin, Role.Leader, Role.Member, Role.Minister] },
  { label: "Perfil", route: "/profile", icon: "fa-solid fa-user", roles: [Role.Admin, Role.Leader, Role.Member, Role.Minister] },
  { label: "Usuários", route: "/users", icon: "fa-solid fa-users", roles: [Role.Admin, Role.Leader] },
  { label: "Configurações", route: "/settings", icon: "fa-solid fa-cog", roles: [Role.Admin, Role.Leader, Role.Member, Role.Minister] },
  { label: "Sair", route: "/logout", icon: "fa-solid fa-sign-out-alt", roles: [Role.Admin, Role.Leader, Role.Member, Role.Minister] }
];

const visibleMenuItems = computed(() =>
  menuItems.filter(item => authStore.hasAnyRole(item.roles))
);

const toggleLeftDrawer = () => {
  drawer.value = !drawer.value;
};

const goTo = async (path) => {
  if (path === '/logout') {
    logoutDialog.value = true;
    return;
  }

  if (path !== route.path) {
    router.push(path);
  }
  drawer.value = false; // Fecha o menu ao navegar
};

const logout = async () => {
  try {
    await api.post('auths/logout').finally(() => {
      logoutDialog.value = false;
      authStore.clearUser();
      router.push('/login');
    });
  } catch (error) {
    console.error('Erro ao sair:', error);
  }
};
</script>