<template>
  <q-page class="q-pa-md">
    <div class="row q-col-gutter-md">
      <div class="col-12">
        <div class="text-h6 q-mb-md">Configurações do Sistema</div>
      </div>

      <!-- Seção de Notificações -->
      <div class="col-12 col-md-6">
        <q-card flat bordered>
          <q-card-section>
            <div class="text-subtitle1 q-mb-sm">Notificações do Dispositivo</div>
            
            <div v-if="isSupported">
              <q-banner :class="permissionGranted ? 'bg-positive text-white' : 'bg-grey-2 text-black'" rounded>
                <template v-slot:avatar>
                  <q-icon :name="permissionGranted ? 'fa-solid fa-bell' : 'fa-solid fa-bell-slash'" :color="permissionGranted ? 'white' : 'primary'" />
                </template>
                <div>Notificações Push</div>
                <div class="text-caption" v-if="!permissionGranted">
                  Mantenha-se informado sobre convites e escalas ativando as notificações.
                </div>
                <div class="text-caption" v-else>
                  As notificações estão ativas para este dispositivo!
                </div>
                
                <template v-slot:action v-if="!permissionGranted">
                  <q-btn flat dense label="Ativar Notificações" class="bg-primary text-white q-px-md" @click="requestPermission" />
                </template>
              </q-banner>
            </div>

            <div v-else>
              <q-banner class="bg-warning text-black" rounded>
                <template v-slot:avatar>
                  <q-icon name="fa-solid fa-circle-info" color="black" />
                </template>
                <div class="text-subtitle2">Push não habilitado</div>
                <div class="text-caption">
                  Para ativar notificações no celular (iOS/Android), você deve primeiro <strong>instalar o App</strong> (Adicionar à Tela Inicial) e abri-lo pelo ícone gerado.
                </div>
              </q-banner>
            </div>
          </q-card-section>
        </q-card>
      </div>

      <!-- Outras configurações futuras podem ser adicionadas aqui -->
    </div>
  </q-page>
</template>

<script setup>
import { useNotifications } from '../composables/useNotifications';

const { isSupported, permissionGranted, requestPermission } = useNotifications();
</script>

<style scoped>
</style>
