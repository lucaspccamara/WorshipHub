<template>
  <div class="row items-center q-pa-sm bg-white text-dark sticky-header" style="border-bottom: 1px solid var(--q-separator); min-height: 50px; z-index: 10;">
    <q-icon v-if="icon" :name="icon" size="xs" class="q-ml-sm q-mr-sm text-primary" />
    <div class="text-subtitle1 text-weight-bold row items-center">
      {{ title }}
      <slot name="subtitle">
        <span v-if="subtitle" class="text-weight-regular text-grey-7 q-ml-sm">{{ subtitle }}</span>
      </slot>
    </div>
    <q-space />
    <div class="row q-gutter-x-sm items-center">
      <slot name="actions"></slot>
      <q-btn v-if="showClose" dense flat icon="fa fa-close" v-close-popup class="q-ml-xs">
        <q-tooltip v-if="closeTooltip">{{ closeTooltip }}</q-tooltip>
      </q-btn>
    </div>
  </div>
</template>

<script setup>
defineProps({
  title: {
    type: String,
    required: true
  },
  subtitle: {
    type: String,
    default: ''
  },
  icon: {
    type: String,
    default: ''
  },
  showClose: {
    type: Boolean,
    default: false
  },
  closeTooltip: {
    type: String,
    default: 'Fechar'
  }
})
</script>

<style scoped>
.sticky-header {
  position: sticky;
  top: 0;
  width: 100%;
}
</style>
