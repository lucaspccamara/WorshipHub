<template>
  <q-card>
    <form @submit.prevent="createEvents">
      <q-bar class="card-header">
        <span>Cadastrar Escala</span>
        <q-space />
        <q-btn dense flat icon="fa fa-close" v-close-popup>
          <q-tooltip>Fechar</q-tooltip>
        </q-btn>
      </q-bar>

      <div class="row">
        <q-card-section :class="['col-12', 'col-md-5', 'column-left']">
          <q-date
            v-model="selectedDate"
            :events="events.map(event => event.date)"
            :event-color="getEventColor"
            class="input-date"
          />
          <q-select
            v-model="eventType"
            @update:model-value="val => eventType = val ? val.value : null"
            :options="eventTypes"
            map-options
            outlined
            dense
            clearable
            label="Tipo de Evento"
            class="q-my-md"
          />
          <q-btn color="primary" label="Adicionar Evento" @click="addEvent()" />
          <q-btn
            v-if="isMobile && events.length > 0"
            class="q-mt-md"
            color="secondary"
            label="Eventos Selecionados"
            @click="showMobileEventsDialog = true"
          />
        </q-card-section>
  
        <div :class="['col-12', 'col-md-7']" v-if="!isMobile">
          <q-card-section class="column-right">
            <div v-if="events.length > 0">
              <q-card v-for="(event, index) in events" :key="index" class="q-mb-md">
                <q-card-section class="row items-center justify-between">
                  <div>
                    <p><strong>Data:</strong> {{ formatDate(event.date) }}</p>
                    <p><strong>Tipo de Evento:</strong> {{ event.label }}</p>
                  </div>

                  <q-btn
                    align="right"
                    color="negative"
                    flat
                    dense
                    icon="fa fa-trash"
                    @click="removeEvent(index)"
                  />
                </q-card-section>
              </q-card>
            </div>
          </q-card-section>
          
          <q-card-actions class="card-footer position-absolute">
            <q-btn color="primary" type="submit" label="Cadastrar Escala" class="align-right-bottom q-ma-md" />
          </q-card-actions>
        </div>
      </div>
    </form>
  </q-card>

  <q-dialog v-model="showMobileEventsDialog" full-width>
    <q-card class="scrolling-dialog">
      <q-bar class="card-header">
        <div class="text-h6">Eventos Selecionados</div>
        <q-space />
        <q-btn dense flat icon="fa fa-close" v-close-popup>
          <q-tooltip>Fechar</q-tooltip>
        </q-btn>
      </q-bar>

      <q-separator />

      <q-card-section class="events-scroll">
        <q-card
          v-for="(event, index) in events"
          :key="index"
          class="q-mb-md"
        >
          <q-card-section class="row items-center justify-between">
            <div>
              <p><strong>Data:</strong> {{ formatDate(event.date) }}</p>
              <p><strong>Tipo de Evento:</strong> {{ event.label }}</p>
            </div>
            
            <q-btn
              align="right"
              color="negative"
              flat
              dense
              icon="fa fa-trash"
              @click="removeEvent(index)"
            />
          </q-card-section>
        </q-card>
      </q-card-section>

      <q-card-actions align="right">
        <q-btn
          v-if="isMobile"
          class="full-width q-mt-md"
          color="primary"
          type="submit"
          label="Cadastrar Escala"
        />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup>
import api from '../api';
import { ref } from 'vue';
import { Notify, useQuasar } from 'quasar';
import { EventTypes } from '../constants/EventTypes';

const $q = useQuasar();

const isMobile = $q.screen.lt.md;
const showMobileEventsDialog = ref(false);

const selectedDate = ref(null);
const eventType = ref(null);
const eventTypes = EventTypes;
let events = ref([]);

function addEvent() {
  if (selectedDate.value && eventType.value !== null) {
    const exists = events.value.some(event => 
      event.date === selectedDate.value && event.type === eventTypes.find(et => et.value === eventType.value)?.value
    );

    if (exists) {
      selectedDate.value = null;
      eventType.value = null;
      Notify.create({
        message: 'Já existe um evento com a mesma data e tipo!',
        color: 'negative'
      });
      return;
    }

    events.value.push({
      date: selectedDate.value,
      label: eventTypes.find(event => event.value === eventType.value).label,
      type: eventType.value,
      color: eventTypes.find(event => event.value === eventType.value).color
    });

    events.value.sort((a, b) => {
      if (a.date < b.date) return -1;
      if (a.date > b.date) return 1;

      return a.type - b.type;
    });

    selectedDate.value = null;
    eventType.value = null;

    Notify.create({
      message: 'Evento adicionado com sucesso!',
      color: 'positive'
    });
  } else {
    Notify.create({
      message: 'Por favor, selecione uma data e um tipo de evento.',
      color: 'negative'
    });
  }
}

function removeEvent(index) {
  events.value.splice(index, 1);

  if (events.value.length === 0) {
    showMobileEventsDialog.value = false;
  }
}

function formatDate(date) {
  const [year, month, day] = date.split('/');
  return `${day}/${month}/${year.slice(2)}`;
}

function getEventColor(date) {
  const eventOnDate = events.value.filter(event => event.date === date);
  return eventOnDate.map(event => event.color);
}

function createEvents() {
  const mappedEvents = events.value.map(event => ({
    date: event.date,
    eventType: event.type
  }));

  api.create('schedules', mappedEvents);
}
</script>

<style lang="scss">
.column-left {
  border-right: 1px solid #ccc;
}

.column-right {
  height: 100%;
  max-height: 450px;
  overflow-y: auto;
}

.input-date {
  width: 100%;
  min-width: none;
}

.position-absolute {
  position: absolute;
  bottom: 0;
  right: 0;
  width: 50%;
}

.align-right-bottom {
  position: absolute;
  bottom: 0;
  right: 0;
}

.scrolling-dialog {
  max-height: 80vh;
  display: flex;
  flex-direction: column;
}

.events-scroll {
  overflow-y: auto;
  flex: 1;
}

@media (max-width: 768px) {
  .position-absolute,
  .align-right-bottom {
    position: static !important;
    width: 100% !important;
  }

  .column-left {
    border: none !important;
  }
}
</style>