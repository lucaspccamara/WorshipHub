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
        v-for="(item, index) in menuItems"
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
</template>

<script setup>
import { ref, computed } from "vue";
import { useRouter, useRoute } from "vue-router";

const drawer = ref(false);
const router = useRouter();
const route = useRoute();
const showMenu = computed(() => route.path !== "/login");

const menuItems = [
  { label: "Escalas", route: "/schedule", icon: "fa-solid fa-calendar" },
  { label: "Perfil", route: "/profile", icon: "fa-solid fa-user" },
  { label: "Configurações", route: "/settings", icon: "fa-solid fa-cog" }
];

const toggleLeftDrawer = () => {
  drawer.value = !drawer.value;
};

const goTo = (path) => {
  if (path !== route.path) {
    router.push(path);
  }
  drawer.value = false; // Fecha o menu ao navegar
};
</script>