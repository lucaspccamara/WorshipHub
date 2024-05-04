<template>
  <q-layout>
    <q-page-container>
      <!-- Menu lateral -->
      <q-drawer
        v-model="drawer"
        show-if-above
        bordered
        content-class="bg-grey-2"
      >
        <q-scroll-area>
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
        </q-scroll-area>
      </q-drawer>

      <!-- Conteúdo da página -->
      <q-page-container>
        <q-page class="q-pa-md">
          <div class="text-h6">Dashboard</div>
          <!-- Calendário -->
          <q-date v-model="selectedDate" mask="YYYY-MM-DD"></q-date>
          <!-- Lista de músicas da escala -->
          <q-list>
            <q-item
              v-for="(music, index) in schedule"
              :key="index"
            >
              <q-item-label>{{ music }}</q-item-label>
            </q-item>
          </q-list>
        </q-page>
      </q-page-container>
    </q-page-container>
  </q-layout>
</template>

<script>
import { ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { Notify } from 'quasar';
//import moment from 'moment';

export default {
  name: 'Home',
  setup() {
    const drawer = ref(true);
    const selectedDate = ref(new Date());
    const schedule = ref({
      '2022-05-01': ['Música 1', 'Música 2'],
      '2022-05-02': ['Música 3', 'Música 4']
    });

    const menuItems = [
      { label: 'Home', route: '/home' },
      { label: 'Perfil', route: '/profile' },
      { label: 'Configurações', route: '/settings' }
    ];

    const route = useRoute();
    const router = useRouter();

    const goTo = (route) => {
      if (route !== route.path) {
        router.push(route);
      }
    };

    const showSchedule = (date) => {
      Notify.create({
        message: `Você está escalado no dia`,
        color: 'info'
      });
    };

    return {
      drawer,
      selectedDate,
      schedule,
      menuItems,
      goTo,
      showSchedule
    };
  }
};
</script>

<style scoped>
/* Estilos personalizados */
</style>
