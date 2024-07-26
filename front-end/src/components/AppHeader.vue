<template>
  <q-header elevated class="bg-primary text-white">
    <q-toolbar>
      <q-btn dense flat round icon="fa fa-bars" @click="toggleLeftDrawer" />
      <q-toolbar-title>
        <q-avatar>
          <img src="https://cdn.quasar.dev/logo-v2/svg/logo-mono-white.svg" alt="Logo" />
        </q-avatar>
        WorshipHub
      </q-toolbar-title>
    </q-toolbar>
  </q-header>

  <q-drawer v-model="drawer" side="left" behavior="mobile" elevated>
    <q-list>
      <q-item
        v-for="(item, index) in menuItems"
        :key="index"
        clickable
        @click="goTo(item.route)"
      >
        <q-item-section>
          <q-item-label>{{ item.label }}</q-item-label>
        </q-item-section>
      </q-item>
    </q-list>
  </q-drawer>
</template>

<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';

const drawer = ref(false);
const router = useRouter();
const menuItems = [
  { label: 'Escalas', route: '/schedule' },
  { label: 'Perfil', route: '/profile' },
  { label: 'Configurações', route: '/settings' }
];

const toggleLeftDrawer = () => {
  drawer.value = !drawer.value;
};

const goTo = (route) => {
  if (route !== route.path) {
    router.push(route);
  }
};
</script>