<template>
  <q-card class="column full-height bg-white">
    <AppSectionHeader 
      v-if="!hideHeader" 
      title="Organização da Escala" 
      icon="fa-solid fa-people-group" 
      show-close 
    />

    <q-card class="col column q-pa-none" flat>
      <q-card-section class="col overflow-auto q-pa-sm q-pt-none">

        <div v-if="loading" class="row items-center justify-center q-pa-lg">
          <q-spinner-dots size="30px" color="primary" />
        </div>

        <div v-else>
          <!-- TABS ROW -->
          <q-tabs
            v-model="tab"
            dense
            class="text-grey"
            active-color="primary"
            indicator-color="primary"
            align="left"
            narrow-indicator
            outside-arrows
            mobile-arrows
          >
            <q-tab name="geral" label="Visão Geral" />
            <q-tab
              v-for="s in schedules"
              :key="'tab-' + s.scheduleId"
              :name="String(s.scheduleId)"
              :label="formatDate(s.date)"
            >
              <q-badge v-if="scheduleWarnings.some(w => w.scheduleId === s.scheduleId)" color="orange" floating transparent>!</q-badge>
            </q-tab>
          </q-tabs>

          <q-separator />

          <q-tab-panels v-model="tab" animated class="bg-transparent">
            <!-- ABA GERAL -->
            <q-tab-panel name="geral" class="q-pa-sm">
              <div class="row q-col-gutter-md">
                <div class="col-12 col-md-6">
                  <q-expansion-item
                    icon="fa fa-chart-bar"
                    label="Frequência de Escalação"
                    header-class="bg-grey-1 text-subtitle2 text-weight-bold"
                    class="overflow-hidden bg-white"
                    style="border: 1px solid var(--q-separator); border-radius: 4px;"
                  >
                    <q-separator />
                    <div class="q-pa-sm" style="max-height: 300px; overflow-y: auto;">
                      <div v-if="memberFrequency.length > 0" class="row q-gutter-xs">
                        <q-chip
                          v-for="mf in memberFrequency"
                          :key="mf.id"
                          dense
                          color="blue-1"
                          text-color="primary"
                        >
                          {{ mf.name }}
                          <q-badge color="primary" align="middle" class="q-ml-xs">{{ mf.count }}</q-badge>
                        </q-chip>
                      </div>
                      <div v-else class="text-grey q-pa-sm text-center">Ninguém escalado ainda.</div>
                    </div>
                  </q-expansion-item>
                </div>
                
                <div class="col-12 col-md-6">
                  <q-expansion-item
                    icon="fa fa-exclamation-triangle"
                    label="Alertas e Pendências"
                    header-class="bg-grey-1 text-subtitle2 text-weight-bold"
                    class="overflow-hidden bg-white"
                    style="border: 1px solid var(--q-separator); border-radius: 4px;"
                  >
                    <q-separator />
                    <div class="q-pa-xs" style="max-height: 300px; overflow-y: auto;">
                      <q-list dense v-if="scheduleWarnings.length > 0">
                        <q-item v-for="(w, idx) in scheduleWarnings" :key="idx">
                          <q-item-section avatar min-width="auto" class="q-pr-sm">
                            <q-icon :name="w.type === 'conflict' ? 'fa fa-times-circle' : 'fa fa-info-circle'" :color="w.type === 'conflict' ? 'negative' : 'warning'" size="xs" />
                          </q-item-section>
                          <q-item-section>
                            <q-item-label :class="w.type === 'conflict' ? 'text-negative text-weight-bold' : 'text-grey-8'">{{ w.message }}</q-item-label>
                          </q-item-section>
                        </q-item>
                      </q-list>
                      <div v-else class="text-green q-pa-sm text-center"><q-icon name="fa fa-check" /> Tudo certo por aqui!</div>
                    </div>
                  </q-expansion-item>
                </div>
              </div>

              <!-- ESCALA GERAL - LOUVOR -->
              <div class="text-subtitle2 text-primary q-mt-lg q-mb-sm"><q-icon name="fa fa-music" /> Escala Geral - Equipe de Louvor</div>
              
              <!-- Desktop Louvor Geral -->
              <div class="visible-md block" v-if="$q.screen.gt.sm">
                <q-markup-table flat bordered dense separator="cell">
                  <thead>
                    <tr>
                      <th class="text-left bg-grey-3">Data</th>
                      <th v-for="col in worshipPositions" :key="'gth-w-'+col.value" class="text-left bg-grey-2">{{ col.label }}</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="sched in schedules" :key="'gtr-w-'+sched.scheduleId">
                      <td class="text-weight-bold bg-grey-1" style="vertical-align: middle;">{{ formatDate(sched.date) }}</td>
                      <td v-for="col in worshipPositions" :key="'gtd-w-'+sched.scheduleId+'-'+col.value" style="vertical-align: top; min-width: 130px; padding: 4px;">
                        <q-select
                          :readonly="!canEdit"
                          :ref="el => setSelectRef(sched.scheduleId, col.value, el)"
                          dense outlined multiple use-chips
                          :options="membersByPosition[col.value] || []"
                          option-value="id" option-label="name" emit-value map-options
                          :model-value="assignments[sched.scheduleId]?.[col.value] || []"
                          @update:model-value="val => onSelect(sched.scheduleId, col.value, val)"
                          placeholder="Sel."
                        >
                          <template v-slot:option="scope">
                            <q-item v-bind="scope.itemProps">
                              <q-item-section avatar>
                                <span :class="availDotClass(getMemberAvailability(sched.scheduleId, scope.opt.id))" class="avail-dot" />
                              </q-item-section>
                              <q-item-section>
                                <q-item-label>{{ scope.opt.name }}</q-item-label>
                                <q-item-label caption>{{ availLabel(getMemberAvailability(sched.scheduleId, scope.opt.id)) }}</q-item-label>
                              </q-item-section>
                            </q-item>
                          </template>
                          <template v-slot:selected-item="scope">
                            <q-chip
                              :removable="canEdit" dense
                              :color="getMemberAvailability(sched.scheduleId, scope.opt.id) === false ? 'red-1' : undefined"
                              :text-color="getMemberAvailability(sched.scheduleId, scope.opt.id) === false ? 'negative' : undefined"
                              :outlined="getMemberAvailability(sched.scheduleId, scope.opt.id) === false"
                              @remove="scope.removeAtIndex(scope.index)" class="q-ma-xs"
                            >
                              {{ scope.opt.name }}
                            </q-chip>
                          </template>
                        </q-select>
                      </td>
                    </tr>
                  </tbody>
                </q-markup-table>
              </div>

              <!-- Mobile Louvor Geral -->
              <div class="visible-xs block" v-else>
                <q-card v-for="sched in schedules" :key="'gm-w-'+sched.scheduleId" flat bordered class="q-mb-md">
                  <q-card-section class="bg-grey-2 q-py-xs">
                    <div class="text-weight-bold text-primary">{{ formatDate(sched.date) }}</div>
                  </q-card-section>
                  <q-card-section class="q-pa-sm">
                    <div v-for="pos in worshipPositions" :key="'gm-w-'+sched.scheduleId+'-'+pos.value" class="q-mb-sm">
                      <div class="text-caption q-mb-xs">{{ pos.label }}</div>
                      <q-select
                        :readonly="!canEdit"
                        :ref="el => setSelectRef(sched.scheduleId, pos.value, el)"
                        dense outlined multiple use-chips
                        :options="membersByPosition[pos.value] || []"
                        option-value="id" option-label="name" emit-value map-options
                        :model-value="assignments[sched.scheduleId]?.[pos.value] || []"
                        @update:model-value="val => onSelect(sched.scheduleId, pos.value, val)"
                        :placeholder="`Selecionar ${pos.label}`"
                      >
                        <template v-slot:option="scope">
                          <q-item v-bind="scope.itemProps">
                            <q-item-section avatar>
                              <span :class="availDotClass(getMemberAvailability(sched.scheduleId, scope.opt.id))" class="avail-dot" />
                            </q-item-section>
                            <q-item-section>
                              <q-item-label>{{ scope.opt.name }}</q-item-label>
                              <q-item-label caption>{{ availLabel(getMemberAvailability(sched.scheduleId, scope.opt.id)) }}</q-item-label>
                            </q-item-section>
                          </q-item>
                        </template>
                        <template v-slot:selected-item="scope">
                          <q-chip
                            :removable="canEdit" dense
                            :color="getMemberAvailability(sched.scheduleId, scope.opt.id) === false ? 'red-1' : undefined"
                            :text-color="getMemberAvailability(sched.scheduleId, scope.opt.id) === false ? 'negative' : undefined"
                            :outlined="getMemberAvailability(sched.scheduleId, scope.opt.id) === false"
                            @remove="scope.removeAtIndex(scope.index)" class="q-ma-xs"
                          >
                            {{ scope.opt.name }}
                          </q-chip>
                        </template>
                      </q-select>
                    </div>
                  </q-card-section>
                </q-card>
              </div>

              <!-- ESCALA GERAL - PRODUÇÃO -->
              <div class="text-subtitle2 text-secondary q-mt-lg q-mb-sm"><q-icon name="fa fa-video" /> Escala Geral - Equipe de Produção</div>
              
              <!-- Desktop Produção Geral -->
              <div class="visible-md block" v-if="$q.screen.gt.sm">
                <q-markup-table flat bordered dense separator="cell">
                  <thead>
                    <tr>
                      <th class="text-left bg-grey-3">Data</th>
                      <th v-for="col in productionPositions" :key="'gth-p-'+col.value" class="text-left bg-grey-2">{{ col.label }}</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="sched in schedules" :key="'gtr-p-'+sched.scheduleId">
                      <td class="text-weight-bold bg-grey-1" style="vertical-align: middle;">{{ formatDate(sched.date) }}</td>
                      <td v-for="col in productionPositions" :key="'gtd-p-'+sched.scheduleId+'-'+col.value" style="vertical-align: top; min-width: 130px; padding: 4px;">
                        <q-select
                          :readonly="!canEdit"
                          :ref="el => setSelectRef(sched.scheduleId, col.value, el)"
                          dense outlined multiple use-chips
                          :options="membersByPosition[col.value] || []"
                          option-value="id" option-label="name" emit-value map-options
                          :model-value="assignments[sched.scheduleId]?.[col.value] || []"
                          @update:model-value="val => onSelect(sched.scheduleId, col.value, val)"
                          placeholder="Sel."
                        >
                          <template v-slot:option="scope">
                            <q-item v-bind="scope.itemProps">
                              <q-item-section avatar>
                                <span :class="availDotClass(getMemberAvailability(sched.scheduleId, scope.opt.id))" class="avail-dot" />
                              </q-item-section>
                              <q-item-section>
                                <q-item-label>{{ scope.opt.name }}</q-item-label>
                                <q-item-label caption>{{ availLabel(getMemberAvailability(sched.scheduleId, scope.opt.id)) }}</q-item-label>
                              </q-item-section>
                            </q-item>
                          </template>
                          <template v-slot:selected-item="scope">
                            <q-chip
                              :removable="canEdit" dense
                              :color="getMemberAvailability(sched.scheduleId, scope.opt.id) === false ? 'red-1' : undefined"
                              :text-color="getMemberAvailability(sched.scheduleId, scope.opt.id) === false ? 'negative' : undefined"
                              :outlined="getMemberAvailability(sched.scheduleId, scope.opt.id) === false"
                              @remove="scope.removeAtIndex(scope.index)" class="q-ma-xs"
                            >
                              {{ scope.opt.name }}
                            </q-chip>
                          </template>
                        </q-select>
                      </td>
                    </tr>
                  </tbody>
                </q-markup-table>
              </div>

              <!-- Mobile Produção Geral -->
              <div class="visible-xs block" v-else>
                <q-card v-for="sched in schedules" :key="'gm-p-'+sched.scheduleId" flat bordered class="q-mb-md">
                  <q-card-section class="bg-grey-2 q-py-xs">
                    <div class="text-weight-bold text-secondary">{{ formatDate(sched.date) }}</div>
                  </q-card-section>
                  <q-card-section class="q-pa-sm">
                    <div v-for="pos in productionPositions" :key="'gm-p-'+sched.scheduleId+'-'+pos.value" class="q-mb-sm">
                      <div class="text-caption q-mb-xs">{{ pos.label }}</div>
                      <q-select
                        :readonly="!canEdit"
                        :ref="el => setSelectRef(sched.scheduleId, pos.value, el)"
                        dense outlined multiple use-chips
                        :options="membersByPosition[pos.value] || []"
                        option-value="id" option-label="name" emit-value map-options
                        :model-value="assignments[sched.scheduleId]?.[pos.value] || []"
                        @update:model-value="val => onSelect(sched.scheduleId, pos.value, val)"
                        :placeholder="`Selecionar ${pos.label}`"
                      >
                        <template v-slot:option="scope">
                          <q-item v-bind="scope.itemProps">
                            <q-item-section avatar>
                              <span :class="availDotClass(getMemberAvailability(sched.scheduleId, scope.opt.id))" class="avail-dot" />
                            </q-item-section>
                            <q-item-section>
                              <q-item-label>{{ scope.opt.name }}</q-item-label>
                              <q-item-label caption>{{ availLabel(getMemberAvailability(sched.scheduleId, scope.opt.id)) }}</q-item-label>
                            </q-item-section>
                          </q-item>
                        </template>
                        <template v-slot:selected-item="scope">
                          <q-chip
                            :removable="canEdit" dense
                            :color="getMemberAvailability(sched.scheduleId, scope.opt.id) === false ? 'red-1' : undefined"
                            :text-color="getMemberAvailability(sched.scheduleId, scope.opt.id) === false ? 'negative' : undefined"
                            :outlined="getMemberAvailability(sched.scheduleId, scope.opt.id) === false"
                            @remove="scope.removeAtIndex(scope.index)" class="q-ma-xs"
                          >
                            {{ scope.opt.name }}
                          </q-chip>
                        </template>
                      </q-select>
                    </div>
                  </q-card-section>
                </q-card>
              </div>

            </q-tab-panel>

            <!-- ABAS DAS ESCALAS -->
            <q-tab-panel v-for="item in schedules" :key="'panel-' + item.scheduleId" :name="String(item.scheduleId)" class="q-pa-sm">
              <div class="text-body1 text-weight-bold q-mb-md">Escala de {{ formatDate(item.date) }}</div>

              <!-- EQUIPE DE LOUVOR -->
              <div class="text-subtitle2 text-primary q-mb-sm"><q-icon name="fa fa-music" /> Equipe de Louvor</div>
              
              <!-- Desktop Louvor -->
              <div class="visible-md block" v-if="$q.screen.gt.sm">
                <q-markup-table flat bordered dense separator="cell">
                  <thead>
                    <tr>
                      <th v-for="col in worshipPositions" :key="'th-w-'+col.value" class="text-left bg-grey-2">{{ col.label }}</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr>
                      <td v-for="col in worshipPositions" :key="'td-w-'+col.value" style="vertical-align: top; min-width: 130px; padding: 4px;">
                        <q-select
                          :readonly="!canEdit"
                          :ref="el => setSelectRef(item.scheduleId, col.value, el)"
                          dense outlined multiple use-chips
                          :options="membersByPosition[col.value] || []"
                          option-value="id" option-label="name" emit-value map-options
                          :model-value="assignments[item.scheduleId]?.[col.value] || []"
                          @update:model-value="val => onSelect(item.scheduleId, col.value, val)"
                          placeholder="Sel."
                        >
                          <template v-slot:option="scope">
                            <q-item v-bind="scope.itemProps">
                              <q-item-section avatar>
                                <span :class="availDotClass(getMemberAvailability(item.scheduleId, scope.opt.id))" class="avail-dot" />
                              </q-item-section>
                              <q-item-section>
                                <q-item-label>{{ scope.opt.name }}</q-item-label>
                                <q-item-label caption>{{ availLabel(getMemberAvailability(item.scheduleId, scope.opt.id)) }}</q-item-label>
                              </q-item-section>
                            </q-item>
                          </template>
                          <template v-slot:selected-item="scope">
                            <q-chip
                              :removable="canEdit" dense
                              :color="getMemberAvailability(item.scheduleId, scope.opt.id) === false ? 'red-1' : undefined"
                              :text-color="getMemberAvailability(item.scheduleId, scope.opt.id) === false ? 'negative' : undefined"
                              :outlined="getMemberAvailability(item.scheduleId, scope.opt.id) === false"
                              @remove="scope.removeAtIndex(scope.index)" class="q-ma-xs"
                            >
                              {{ scope.opt.name }}
                            </q-chip>
                          </template>
                        </q-select>
                      </td>
                    </tr>
                  </tbody>
                </q-markup-table>
              </div>

              <!-- Mobile Louvor -->
              <div class="visible-xs block" v-else>
                <div v-for="pos in worshipPositions" :key="'m-w-'+pos.value" class="q-mb-sm">
                  <div class="text-caption q-mb-xs">{{ pos.label }}</div>
                  <q-select
                    :readonly="!canEdit"
                    :ref="el => setSelectRef(item.scheduleId, pos.value, el)"
                    dense outlined multiple use-chips
                    :options="membersByPosition[pos.value] || []"
                    option-value="id" option-label="name" emit-value map-options
                    :model-value="assignments[item.scheduleId]?.[pos.value] || []"
                    @update:model-value="val => onSelect(item.scheduleId, pos.value, val)"
                    :placeholder="`Selecionar ${pos.label}`"
                  >
                    <template v-slot:option="scope">
                      <q-item v-bind="scope.itemProps">
                        <q-item-section avatar>
                          <span :class="availDotClass(getMemberAvailability(item.scheduleId, scope.opt.id))" class="avail-dot" />
                        </q-item-section>
                        <q-item-section>
                          <q-item-label>{{ scope.opt.name }}</q-item-label>
                          <q-item-label caption>{{ availLabel(getMemberAvailability(item.scheduleId, scope.opt.id)) }}</q-item-label>
                        </q-item-section>
                      </q-item>
                    </template>
                    <template v-slot:selected-item="scope">
                      <q-chip
                        :removable="canEdit" dense
                        :color="getMemberAvailability(item.scheduleId, scope.opt.id) === false ? 'red-1' : undefined"
                        :text-color="getMemberAvailability(item.scheduleId, scope.opt.id) === false ? 'negative' : undefined"
                        :outlined="getMemberAvailability(item.scheduleId, scope.opt.id) === false"
                        @remove="scope.removeAtIndex(scope.index)" class="q-ma-xs"
                      >
                        {{ scope.opt.name }}
                      </q-chip>
                    </template>
                  </q-select>
                </div>
              </div>

              <!-- EQUIPE DE PRODUÇÃO -->
              <div class="text-subtitle2 text-secondary q-mt-lg q-mb-sm"><q-icon name="fa fa-video" /> Equipe de Produção</div>
              
              <!-- Desktop Produção -->
              <div class="visible-md block" v-if="$q.screen.gt.sm">
                <q-markup-table flat bordered dense separator="cell">
                  <thead>
                    <tr>
                      <th v-for="col in productionPositions" :key="'th-p-'+col.value" class="text-left bg-grey-2">{{ col.label }}</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr>
                      <td v-for="col in productionPositions" :key="'td-p-'+col.value" style="vertical-align: top; min-width: 130px; padding: 4px;">
                        <q-select
                          :readonly="!canEdit"
                          :ref="el => setSelectRef(item.scheduleId, col.value, el)"
                          dense outlined multiple use-chips
                          :options="membersByPosition[col.value] || []"
                          option-value="id" option-label="name" emit-value map-options
                          :model-value="assignments[item.scheduleId]?.[col.value] || []"
                          @update:model-value="val => onSelect(item.scheduleId, col.value, val)"
                          placeholder="Sel."
                        >
                          <template v-slot:option="scope">
                            <q-item v-bind="scope.itemProps">
                              <q-item-section avatar>
                                <span :class="availDotClass(getMemberAvailability(item.scheduleId, scope.opt.id))" class="avail-dot" />
                              </q-item-section>
                              <q-item-section>
                                <q-item-label>{{ scope.opt.name }}</q-item-label>
                                <q-item-label caption>{{ availLabel(getMemberAvailability(item.scheduleId, scope.opt.id)) }}</q-item-label>
                              </q-item-section>
                            </q-item>
                          </template>
                          <template v-slot:selected-item="scope">
                            <q-chip
                              :removable="canEdit" dense
                              :color="getMemberAvailability(item.scheduleId, scope.opt.id) === false ? 'red-1' : undefined"
                              :text-color="getMemberAvailability(item.scheduleId, scope.opt.id) === false ? 'negative' : undefined"
                              :outlined="getMemberAvailability(item.scheduleId, scope.opt.id) === false"
                              @remove="scope.removeAtIndex(scope.index)" class="q-ma-xs"
                            >
                              {{ scope.opt.name }}
                            </q-chip>
                          </template>
                        </q-select>
                      </td>
                    </tr>
                  </tbody>
                </q-markup-table>
              </div>

              <!-- Mobile Produção -->
              <div class="visible-xs block" v-else>
                <div v-for="pos in productionPositions" :key="'m-p-'+pos.value" class="q-mb-sm">
                  <div class="text-caption q-mb-xs">{{ pos.label }}</div>
                  <q-select
                    :readonly="!canEdit"
                    :ref="el => setSelectRef(item.scheduleId, pos.value, el)"
                    dense outlined multiple use-chips
                    :options="membersByPosition[pos.value] || []"
                    option-value="id" option-label="name" emit-value map-options
                    :model-value="assignments[item.scheduleId]?.[pos.value] || []"
                    @update:model-value="val => onSelect(item.scheduleId, pos.value, val)"
                    :placeholder="`Selecionar ${pos.label}`"
                  >
                    <template v-slot:option="scope">
                      <q-item v-bind="scope.itemProps">
                        <q-item-section avatar>
                          <span :class="availDotClass(getMemberAvailability(item.scheduleId, scope.opt.id))" class="avail-dot" />
                        </q-item-section>
                        <q-item-section>
                          <q-item-label>{{ scope.opt.name }}</q-item-label>
                          <q-item-label caption>{{ availLabel(getMemberAvailability(item.scheduleId, scope.opt.id)) }}</q-item-label>
                        </q-item-section>
                      </q-item>
                    </template>
                    <template v-slot:selected-item="scope">
                      <q-chip
                        :removable="canEdit" dense
                        :color="getMemberAvailability(item.scheduleId, scope.opt.id) === false ? 'red-1' : undefined"
                        :text-color="getMemberAvailability(item.scheduleId, scope.opt.id) === false ? 'negative' : undefined"
                        :outlined="getMemberAvailability(item.scheduleId, scope.opt.id) === false"
                        @remove="scope.removeAtIndex(scope.index)" class="q-ma-xs"
                      >
                        {{ scope.opt.name }}
                      </q-chip>
                    </template>
                  </q-select>
                </div>
              </div>
            </q-tab-panel>
          </q-tab-panels>
        </div>
      </q-card-section>

      <q-separator v-if="!hideFooter && canEdit" />
      
      <q-card-actions v-if="!hideFooter && canEdit" align="right" class="col-auto bg-white q-pa-md">
        <q-btn color="primary" label="Salvar" @click="save" :loading="saving" />
        <q-btn v-if="showTransition" color="secondary" :label="advanceLabel" @click="saveAndAdvance" :loading="savingAdvance" />
      </q-card-actions>
    </q-card>
  </q-card>
</template>

<script setup>
import { ref, onMounted, computed, nextTick } from 'vue'
import { Notify, useQuasar } from 'quasar'
import AppSectionHeader from './AppSectionHeader.vue';
import api from '../api'
import { PositionOptions } from '../constants/PositionOptions'
import { Role } from '../constants/Role'
import { EScheduleStatus } from '../constants/ScheduleStatus'
import { useAuthStore } from '../stores/authStore'

const authStore = useAuthStore()
const canEdit = computed(() => authStore.hasAnyRole([Role.Admin, Role.Leader]))

const $q = useQuasar()

const props = defineProps({
  scheduleIds: { type: Array, required: false },
  showTransition: { type: Boolean, default: true },
  hideHeader: { type: Boolean, default: false },
  hideFooter: { type: Boolean, default: false },
  showNotify: { type: Boolean, default: true }
})
const emit = defineEmits(['saved','advanced'])

defineExpose({ save })

const loading = ref(false)
const saving = ref(false)
const savingAdvance = ref(false)
const positionOptions = PositionOptions
const tab = ref('geral')

const selectRefs = ref({})
function setSelectRef(sid, pos, el) {
  if (el) selectRefs.value[`${sid}-${pos}`] = el
}

// data structures
const schedules = ref([]) // [{ scheduleId, date, eventType, status }]
const membersByPosition = ref({}) // { positionValue: [{id,name,...}] }
const assignments = ref({}) // { scheduleId: { positionValue: memberId[] } }
const availabilityBySchedule = ref({}) // { scheduleId: { userId: bool|null } }

// --- Availability helpers ---
function getMemberAvailability(scheduleId, userId) {
  const schedMap = availabilityBySchedule.value[scheduleId] ?? availabilityBySchedule.value[String(scheduleId)]
  if (!schedMap) return undefined // not collected for this schedule
  const id = Number(userId)
  if (!(id in schedMap) && !(String(id) in schedMap)) return undefined
  return schedMap[id] ?? schedMap[String(id)] // true | false | null
}

function availDotClass(avail) {
  if (avail === true) return 'avail-dot--yes'
  if (avail === false) return 'avail-dot--no'
  if (avail === null) return 'avail-dot--pending'
  return '' // undefined = availability not collected for this schedule
}

function availLabel(avail) {
  if (avail === true) return 'Disponível'
  if (avail === false) return 'Indisponível'
  if (avail === null) return 'Não respondeu'
  return ''
}

const worshipPositions = computed(() => positionOptions.filter(p => p.value < 50))
const productionPositions = computed(() => positionOptions.filter(p => p.value >= 50))

const nextStatus = computed(() => {
  if (schedules.value.length === 0) return EScheduleStatus.AguardandoRepertorio;
  const current = schedules.value[0].status;
  if (current === EScheduleStatus.Criado) return EScheduleStatus.ColetandoDisponibilidade;
  if (current === EScheduleStatus.ColetandoDisponibilidade) return EScheduleStatus.AguardandoRepertorio;
  return EScheduleStatus.AguardandoRepertorio;
});

const advanceLabel = computed(() => {
  if (nextStatus.value === EScheduleStatus.ColetandoDisponibilidade) return 'Salvar e Iniciar Coleta';
  if (nextStatus.value === EScheduleStatus.AguardandoRepertorio) return 'Salvar e Solicitar Repertório';
  return 'Salvar e Avançar';
});

const memberFrequency = computed(() => {
  const counts = {} // { userId: { name: '', count: 0 } }
  for (const s of schedules.value) {
    const raw = assignments.value[s.scheduleId] || {}
    const membersInSchedule = new Set()
    
    for (const posKey in raw) {
      const arr = raw[posKey] || []
      for (const id of arr) {
        if (!counts[id]) {
          let name = `Usuário ${id}`
          const list = membersByPosition.value[posKey] || []
          const found = list.find(m => m.id === id)
          if (found) name = found.name
          counts[id] = { id, name, count: 0 }
        }
        membersInSchedule.add(id)
      }
    }
    
    // Increment only once per schedule
    membersInSchedule.forEach(id => {
      counts[id].count++
    })
  }
  return Object.values(counts).sort((a,b) => b.count - a.count || a.name.localeCompare(b.name))
})

const scheduleWarnings = computed(() => {
  const warnings = []
  for (const s of schedules.value) {
    const raw = assignments.value[s.scheduleId] || {}
    const dateStr = formatDate(s.date)
    
    // Check conflicts
    for (const posKey in raw) {
      const arr = raw[posKey] || []
      for (const id of arr) {
        if (getMemberAvailability(s.scheduleId, id) === false) {
          let mName = id
          const list = membersByPosition.value[posKey] || []
          const found = list.find(m => m.id === id)
          if (found) mName = found.name
          const pName = positionOptions.find(p => String(p.value) === String(posKey))?.label || posKey
          warnings.push({ type: 'conflict', message: `${mName} está escalado como ${pName} em ${dateStr}, mas marcou Indisponível.`, scheduleId: s.scheduleId })
        }
      }
    }
    
    // Check empty positions
    const emptyPos = []
    for (const pos of positionOptions) {
      const arr = raw[pos.value] || []
      if (arr.length === 0) {
        emptyPos.push(pos.label)
      }
    }
    if (emptyPos.length > 0) {
      warnings.push({ type: 'empty', message: `Posições não preenchidas em ${dateStr}: ${emptyPos.join(', ')}.`, scheduleId: s.scheduleId })
    }
  }
  return warnings
})

function formatDate(d) {
  try { return new Date(d).toLocaleDateString(); } catch { return d }
}

async function load() {
  loading.value = true
  try {
    const ids = (props.scheduleIds && props.scheduleIds.length) ? props.scheduleIds : []
    if (ids.length === 0) {
      Notify.create({ type: 'negative', message: 'Nenhuma escala informada.' })
      loading.value = false
      return
    }

    // request batch
    const resp = await api.post('schedules/assignments/details', ids)
    const dto = resp.data

    // schedules list
    const parsedSchedules = dto.schedules || dto.Schedules || []
    const list = Array.isArray(parsedSchedules) 
      ? parsedSchedules.map(s => ({ scheduleId: s.scheduleId ?? s.ScheduleId, date: s.date ?? s.Date, eventType: s.eventType ?? s.EventType, status: s.status ?? s.Status }))
      : []
    list.sort((a, b) => new Date(a.date) - new Date(b.date));
    schedules.value = list;

    // membersByPosition -> normalize and ensure arrays for all positions
    const raw = dto.membersByPosition || dto.MembersByPosition || {}
    const norm = {}
    // init all position keys so selects always have an array
    positionOptions.forEach(p => { norm[String(p.value)] = [] })

    for (const k in raw) {
      const arr = raw[k] || []
      norm[k] = Array.isArray(arr) ? arr.map(m => ({
        id: m.id ?? m.Id,
        name: m.name ?? m.Name ?? `Usuário ${m.id ?? m.Id}`,
        position: m.position ?? m.Position,
        phoneNumber: m.phoneNumber ?? m.PhoneNumber,
        avatarUrl: m.avatarUrl ?? m.AvatarUrl,
        available: (m.available ?? m.Available) ?? null
      })) : []
    }

    membersByPosition.value = norm

    // store per-schedule availability map
    const rawAvail = dto.availabilityBySchedule || dto.AvailabilityBySchedule || {}
    // normalize keys to numbers
    const normAvail = {}
    for (const sid in rawAvail) {
      normAvail[Number(sid)] = {}
      for (const uid in rawAvail[sid]) {
        const v = rawAvail[sid][uid]
        normAvail[Number(sid)][Number(uid)] = v === null ? null : Boolean(v)
      }
    }
    availabilityBySchedule.value = normAvail

    // init assignments from currentAssignments (per schedule) if provided, else empty
    assignments.value = {}
    const cur = dto.currentAssignments || dto.CurrentAssignments || {}
    for (const s of schedules.value) {
      const sid = String(s.scheduleId)
      const map = {}
      positionOptions.forEach(p => {
        const raw = cur[sid] ?? cur[s.scheduleId]
        const val = raw ? (raw[p.value] ?? raw[String(p.value)]) : null
        // normalize: API may return array or null
        map[p.value] = Array.isArray(val) ? val : (val != null ? [val] : [])
      })
      assignments.value[s.scheduleId] = map
    }

    // ensure assigned ids appear in options so select shows label (create placeholder if missing)
    for (const sid of Object.keys(assignments.value)) {
      const map = assignments.value[sid]
      for (const posKey of Object.keys(map)) {
        const assignedIds = map[posKey] || []
        const list = membersByPosition.value[String(posKey)] || []
        for (const assignedId of assignedIds) {
          if (!list.some(x => x.id === assignedId)) {
            list.push({ id: assignedId, name: `Usuário ${assignedId}`, position: Number(posKey), phoneNumber: '', avatarUrl: '', available: null })
            membersByPosition.value[String(posKey)] = list
          }
        }
      }
    }
  } catch (err) {
    Notify.create({ type: 'negative', message: 'Erro ao carregar dados.' })
  } finally {
    loading.value = false
  }
}

async function onSelect(scheduleId, position, newUserIds) {
  // newUserIds is an array because q-select is multiple
  const newIds = Array.isArray(newUserIds) ? newUserIds : (newUserIds != null ? [newUserIds] : [])

  // Only warn about newly added members that are unavailable FOR THIS SPECIFIC SCHEDULE
  const prev = assignments.value[scheduleId][position] || []
  const added = newIds.filter(id => !prev.includes(id))

  const unavailableAdded = added.filter(id => getMemberAvailability(scheduleId, id) === false)

  if (unavailableAdded.length > 0) {
    const list = membersByPosition.value[String(position)] || membersByPosition.value[position] || []
    const name = (unavailableAdded || [])
      .map(id => list.find(m => m.id === id)?.name ?? `Usuário ${id}`)
      .join(', ')
    const confirmed = await new Promise(resolve => {
      $q.dialog({
        title: 'Confirmar escolha',
        message: `${name} está marcado como indisponível para esta data. Deseja mesmo atribuir?`,
        cancel: true,
        persistent: true,
        ok: { label: 'Sim', color: 'primary' },
        cancel: { label: 'Cancelar' }
      })
      .onOk(() => resolve(true))
      .onCancel(() => resolve(false))
      .onDismiss(() => resolve(false))
    })

    if (!confirmed) {
      await nextTick()
      const filteredIds = newIds.filter(id => !unavailableAdded.includes(id))
      const newAssignments = { ...assignments.value }
      newAssignments[scheduleId] = { ...newAssignments[scheduleId] }
      newAssignments[scheduleId][position] = filteredIds
      assignments.value = newAssignments
      
      const el = selectRefs.value[`${scheduleId}-${position}`]
      if (el) {
        if (typeof el.hidePopup === 'function') el.hidePopup()
        if (typeof el.blur === 'function') el.blur()
      }
      return
    }
  }

  // commit locally (save happens only when user clicks Salvar)
  assignments.value = { ...assignments.value, [scheduleId]: { ...assignments.value[scheduleId], [position]: newIds } }
  
  const el = selectRefs.value[`${scheduleId}-${position}`]
  if (el) {
    if (typeof el.hidePopup === 'function') el.hidePopup()
    if (typeof el.blur === 'function') el.blur()
  }
}

async function save() {
  saving.value = true
  try {
    const ids = (schedules.value || []).map(s => s.scheduleId)
    for (const sid of ids) {
      // send { positionValue: [userId, ...] } — only positions with at least one member
      const raw = assignments.value[sid] || {}
      const payload = { assignments: {} }
      for (const posKey of Object.keys(raw)) {
        const arr = raw[posKey]
        if (Array.isArray(arr) && arr.length > 0) {
          payload.assignments[posKey] = arr
        }
      }
      await api.post(`schedules/${sid}/assignments`, payload)
    }
    if (props.showNotify) {
      Notify.create({ type: 'positive', message: 'Atribuições salvas.' })
    }
    //emit('saved', ids)
  } catch (err) {
    Notify.create({ type: 'negative', message: 'Erro ao salvar.' })
  } finally {
    saving.value = false
  }
}

async function saveAndAdvance() {
  savingAdvance.value = true
  try {
    await save()
    const ids = (schedules.value || []).map(s => s.scheduleId)
    await api.post('schedules/transition', { scheduleIds: ids, newStatus: nextStatus.value })
    Notify.create({ type: 'positive', message: 'Escalas avançadas com sucesso.' })
    emit('advanced', ids)
  } catch {
    Notify.create({ type: 'negative', message: 'Erro ao avançar escalas.' })
  } finally {
    savingAdvance.value = false
  }
}

onMounted(load)
</script>

<style scoped>
.card-date { border: 1px solid var(--q-separator); padding: 8px; border-radius:6px; }
@media (min-width: 600px) {
  .visible-xs { display: none; }
}

/* Availability indicator dot */
.avail-dot {
  display: inline-block;
  width: 10px;
  height: 10px;
  border-radius: 50%;
  border: 1.5px solid rgba(0,0,0,0.12);
  background: transparent;
  flex-shrink: 0;
}
.avail-dot--yes  { background: #4caf50; border-color: #388e3c; } /* green */
.avail-dot--no   { background: #f44336; border-color: #c62828; } /* red */
.avail-dot--pending { background: #9e9e9e; border-color: #616161; } /* gray */

.q-item__section--avatar {
  min-width: fit-content !important;
}
</style>