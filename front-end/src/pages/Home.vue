<template>
  <q-layout view="hHh lpR fFf">
    <q-page-container>
      <q-header elevated class="bg-primary text-white">
        <q-toolbar>
          <q-btn dense flat round icon="fa fa-bars" @click="toggleLeftDrawer" />

          <q-toolbar-title>
            <q-avatar>
              <img src="https://cdn.quasar.dev/logo-v2/svg/logo-mono-white.svg">
            </q-avatar>
            WorshipHub
          </q-toolbar-title>
        </q-toolbar>
      </q-header>

      <q-drawer v-model="drawer" side="left" behavior="mobile" elevated>
        <!-- drawer content -->
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

      <!-- Conteúdo da página -->
      <q-page class="q-pa-md">
        <div class="text-h4">Olá Fulano!</div>
        <div class="text-h6">Fique por dentro do que está por vir...</div>
        <q-layout style="max-width: 1250px; width: 90%; margin: 0 auto;">
          <q-card class="main-card">
            <q-splitter
              v-model="splitterModel"
              horizontal
            >
    
              <template v-slot:before>
                <div class="q-pa-md">
                  <q-date
                    v-model="date"
                    :events="events"
                    event-color="orange"
                    style="width: 100%;"
                  />
                </div>
              </template>
    
              <template v-slot:after>
                <q-tab-panels
                  v-model="date"
                  animated
                  transition-prev="jump-up"
                  transition-next="jump-up"
                >
                  <q-tab-panel name="2024/05/05">
                    <div class="text-h4 q-mb-md">2019/02/01</div>
                    <p>Lorem ipsum dolor sit, amet consectetur adipisicing elit. Quis praesentium cumque magnam odio iure quidem, quod illum numquam possimus obcaecati commodi minima assumenda consectetur culpa fuga nulla ullam. In, libero.</p>
                    <p>Lorem ipsum dolor sit, amet consectetur adipisicing elit. Quis praesentium cumque magnam odio iure quidem, quod illum numquam possimus obcaecati commodi minima assumenda consectetur culpa fuga nulla ullam. In, libero.</p>
                  </q-tab-panel>
    
                  <q-tab-panel name="2024/05/12">
                    <div class="text-h4 q-mb-md">2019/02/05</div>
                    <p>Lorem ipsum dolor sit, amet consectetur adipisicing elit. Quis praesentium cumque magnam odio iure quidem, quod illum numquam possimus obcaecati commodi minima assumenda consectetur culpa fuga nulla ullam. In, libero.</p>
                    <p>Lorem ipsum dolor sit, amet consectetur adipisicing elit. Quis praesentium cumque magnam odio iure quidem, quod illum numquam possimus obcaecati commodi minima assumenda consectetur culpa fuga nulla ullam. In, libero.</p>
                  </q-tab-panel>
    
                  <q-tab-panel name="2024/05/19">
                    <div class="text-h4 q-mb-md">2019/02/06</div>
                    <p>Lorem ipsum dolor sit, amet consectetur adipisicing elit. Quis praesentium cumque magnam odio iure quidem, quod illum numquam possimus obcaecati commodi minima assumenda consectetur culpa fuga nulla ullam. In, libero.</p>
                    <p>Lorem ipsum dolor sit, amet consectetur adipisicing elit. Quis praesentium cumque magnam odio iure quidem, quod illum numquam possimus obcaecati commodi minima assumenda consectetur culpa fuga nulla ullam. In, libero.</p>
                    <p>Lorem ipsum dolor sit, amet consectetur adipisicing elit. Quis praesentium cumque magnam odio iure quidem, quod illum numquam possimus obcaecati commodi minima assumenda consectetur culpa fuga nulla ullam. In, libero.</p>
                  </q-tab-panel>
                </q-tab-panels>
              </template>
            </q-splitter>
          </q-card>
        </q-layout>
      </q-page>
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
    const drawer = ref(false);
    const selectedDate = ref(new Date());
    const schedule = ref({
      '2024-05-05': ['Música 1', 'Música 2'],
      '2024-05-12': ['Música 3', 'Música 4']
    });

    const menuItems = [
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
      splitterModel: ref(50),
      date: ref('2024/05/05'),
      events: [ '2024/05/05', '2024/05/12', '2024/05/19' ],

      drawer,
      selectedDate,
      schedule,
      menuItems,
      goTo,
      showSchedule,
      toggleLeftDrawer () {
        drawer.value = !drawer.value
      }
    };
  }
};
</script>

<style lang="scss">
.main-card{
  height: 100vh !important;
}
</style>
