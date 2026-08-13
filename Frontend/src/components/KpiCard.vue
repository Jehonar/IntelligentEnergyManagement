<template>
  <div class="card flex flex-col justify-between">
    <div class="flex items-center justify-between">
      <p class="text-sm font-medium text-gray-500">{{ title }}</p>
      <component v-if="icon" :is="icon" class="w-6 h-6 text-gray-400" />
    </div>
    <div class="mt-2">
      <span class="text-3xl font-bold" :class="valueClass">
        {{ formattedValue }}
      </span>
      <span class="ml-1 text-sm text-gray-400">{{ unit }}</span>
    </div>
    <p v-if="sub" class="text-xs text-gray-400 mt-1">{{ sub }}</p>
  </div>
</template>

<script setup lang="ts">
import { computed, type Component } from 'vue'

const props = defineProps<{
  title: string
  value: number
  unit?: string
  icon?: Component
  color?: 'blue' | 'indigo' | 'purple' | 'green' | 'red'
  sub?: string
}>()

const formattedValue = computed(() => Number(props.value).toFixed(2))

const valueClass = computed(() => ({
  'text-blue-600':   props.color === 'blue',
  'text-indigo-600': props.color === 'indigo',
  'text-purple-600': props.color === 'purple',
  'text-green-600':  props.color === 'green',
  'text-red-600':    props.color === 'red',
  'text-gray-900':   !props.color,
}))
</script>
