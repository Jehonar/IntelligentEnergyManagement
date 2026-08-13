<template>
  <div class="p-8">
    <div class="mb-8">
      <h1 class="text-2xl font-bold text-gray-900">Rekomandimet AI</h1>
      <p class="text-gray-500 mt-1">Rekomandime për kursimin e energjisë të gjeneruara nga sistemi</p>
    </div>

    <div v-if="loading" class="flex items-center justify-center h-32">
      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
    </div>
    <div v-else-if="error" class="card border-red-200 bg-red-50 text-red-700 text-sm">{{ error }}</div>

    <div v-else-if="!recommendations.length" class="card flex items-center justify-center h-48 text-gray-400">
      <div class="text-center">
        <LightBulbIcon class="w-12 h-12 mx-auto mb-2 text-gray-300" />
        <p>Ende nuk ka rekomandime.</p>
        <p class="text-sm mt-1">Shkoni te <RouterLink to="/prediction" class="text-blue-500 underline">Parashikimi AI</RouterLink> për të krijuar një.</p>
      </div>
    </div>

    <div v-else class="space-y-4">
      <div
        v-for="rec in recommendations"
        :key="rec.id"
        class="card flex gap-4"
      >
        <div class="flex-shrink-0 w-10 h-10 rounded-full flex items-center justify-center bg-gray-100">
          <component :is="iconFor(rec.recommendationType)" class="w-5 h-5 text-gray-600" />
        </div>
        <div class="flex-1">
          <div class="flex items-center justify-between mb-1">
            <span :class="['badge-' + rec.recommendationType.toLowerCase(), 'inline-block px-3 py-1 rounded-full text-sm font-bold']">
              {{ typeLabel(rec.recommendationType) }}
            </span>
            <span class="text-xs text-gray-400">{{ new Date(rec.createdAt).toLocaleString('sq-AL') }}</span>
          </div>
          <p class="text-gray-700 text-sm leading-relaxed">{{ rec.message }}</p>
          <p v-if="rec.predictionId" class="text-xs text-gray-400 mt-1">Bazuar në parashikimin #{{ rec.predictionId }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, type Component } from 'vue'
import { RouterLink } from 'vue-router'
import {
  LightBulbIcon,
  ExclamationTriangleIcon,
  MinusCircleIcon,
  CheckCircleIcon,
  ArrowTrendingDownIcon,
} from '@heroicons/vue/24/outline'
import { getRecommendations, type Recommendation } from '../api'

const recommendations = ref<Recommendation[]>([])
const loading = ref(true)
const error   = ref('')

const typeLabels: Record<string, string> = {
  HIGH: 'I LARTË',
  MODERATE: 'MODERAT',
  NORMAL: 'NORMAL',
  LOW: 'I ULËT',
}

function typeLabel(type: string) {
  return typeLabels[type] ?? type
}

function iconFor(type: string): Component {
  const map: Record<string, Component> = {
    HIGH:     ExclamationTriangleIcon,
    MODERATE: MinusCircleIcon,
    NORMAL:   CheckCircleIcon,
    LOW:      ArrowTrendingDownIcon,
  }
  return map[type] ?? LightBulbIcon
}

onMounted(async () => {
  try {
    recommendations.value = await getRecommendations(20)
  } catch (e: any) {
    error.value = e?.message ?? 'Ngarkimi i rekomandimeve dështoi'
  } finally {
    loading.value = false
  }
})
</script>
