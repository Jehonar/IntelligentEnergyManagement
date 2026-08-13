<template>
  <div class="p-8">
    <div class="mb-8">
      <h1 class="text-2xl font-bold text-gray-900">Ballina</h1>
      <p class="text-gray-500 mt-1">Pasqyra e konsumit të energjisë dhe parashikimeve</p>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center justify-center h-64">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="card border-red-200 bg-red-50 text-red-700">
      <p class="font-semibold">Të dhënat nuk u ngarkuan</p>
      <p class="text-sm mt-1">{{ error }}</p>
      <p class="text-sm mt-2 text-gray-500">Sigurohuni që backend-i po punon në localhost:5000</p>
    </div>

    <template v-else>
      <!-- KPI Cards -->
      <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-5 mb-8">
        <KpiCard
          title="Konsumi sot"
          :value="stats?.latestConsumption ?? 0"
          unit="kWh"
          :icon="BoltIcon"
          color="blue"
        />
        <KpiCard
          title="Mesatarja ditore"
          :value="stats?.averageDailyConsumption ?? 0"
          unit="kWh"
          :icon="ChartBarIcon"
          color="indigo"
        />
        <KpiCard
          title="Parashikimi AI"
          :value="latestPrediction?.predictedConsumption ?? 0"
          unit="kWh"
          :icon="CpuChipIcon"
          color="purple"
          :sub="latestPrediction ? `për ${latestPrediction.predictionDate} në ${latestPrediction.predictionHour}:00` : 'Asnjë parashikim ende'"
        />
        <div class="card flex flex-col justify-between">
          <p class="text-sm font-medium text-gray-500">Statusi i energjisë</p>
          <div class="mt-2">
            <span :class="statusBadgeClass">{{ statusLabel }}</span>
          </div>
          <p class="text-xs text-gray-400 mt-2">Bazuar në parashikimin e fundit vs mesatarja</p>
        </div>
      </div>

      <!-- Charts row -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
        <div class="card">
          <h2 class="text-base font-semibold text-gray-700 mb-4">Konsumi ditor (30 ditët e fundit)</h2>
          <Line v-if="dailyChartData" :data="dailyChartData" :options="lineOptions" />
        </div>
        <div class="card">
          <h2 class="text-base font-semibold text-gray-700 mb-4">Mesatarja e konsumit sipas orës</h2>
          <Bar v-if="hourlyChartData" :data="hourlyChartData" :options="barOptions" />
        </div>
      </div>

      <div class="card">
        <h2 class="text-base font-semibold text-gray-700 mb-4">Konsumi mujor</h2>
        <Bar v-if="monthlyChartData" :data="monthlyChartData" :options="barOptions" />
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { Line, Bar } from 'vue-chartjs'
import {
  Chart as ChartJS, CategoryScale, LinearScale, PointElement,
  LineElement, BarElement, Title, Tooltip, Legend, Filler
} from 'chart.js'
import { BoltIcon, ChartBarIcon, CpuChipIcon } from '@heroicons/vue/24/outline'
import { getStatistics, getPredictionHistory, type EnergyStatistics, type PredictionResponse } from '../api'
import KpiCard from '../components/KpiCard.vue'

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, BarElement, Title, Tooltip, Legend, Filler)

const loading = ref(true)
const error   = ref('')
const stats   = ref<EnergyStatistics | null>(null)
const latestPrediction = ref<PredictionResponse | null>(null)

const statusLabels: Record<string, string> = {
  HIGH: 'I LARTË',
  MODERATE: 'MODERAT',
  NORMAL: 'NORMAL',
  LOW: 'I ULËT',
  UNKNOWN: 'I PANJOHUR',
}

onMounted(async () => {
  try {
    const [s, history] = await Promise.all([getStatistics(), getPredictionHistory(1)])
    stats.value = s
    latestPrediction.value = history[0] ?? null
  } catch (e: any) {
    error.value = e?.message ?? 'Gabim i panjohur'
  } finally {
    loading.value = false
  }
})

const energyStatus = computed(() => {
  if (!stats.value || !latestPrediction.value) return 'UNKNOWN'
  const ratio = latestPrediction.value.predictedConsumption / stats.value.averageDailyConsumption
  if (ratio > 1.2)  return 'HIGH'
  if (ratio > 1.05) return 'MODERATE'
  if (ratio < 0.9)  return 'LOW'
  return 'NORMAL'
})

const statusLabel = computed(() => statusLabels[energyStatus.value] ?? energyStatus.value)

const statusBadgeClass = computed(() => ({
  'badge-high':     energyStatus.value === 'HIGH',
  'badge-moderate': energyStatus.value === 'MODERATE',
  'badge-normal':   energyStatus.value === 'NORMAL',
  'badge-low':      energyStatus.value === 'LOW',
  'inline-block px-3 py-1 rounded-full text-sm font-bold bg-gray-100 text-gray-500': energyStatus.value === 'UNKNOWN',
}))

const dailyChartData = computed(() => {
  if (!stats.value?.dailyData?.length) return null
  return {
    labels: stats.value.dailyData.map(d => d.date.slice(5)),
    datasets: [{
      label: 'Konsumi ditor (kWh)',
      data: stats.value.dailyData.map(d => d.totalConsumption),
      borderColor: '#3b82f6',
      backgroundColor: 'rgba(59,130,246,0.08)',
      fill: true,
      tension: 0.4,
      pointRadius: 3,
    }],
  }
})

const hourlyChartData = computed(() => {
  if (!stats.value?.hourlyData?.length) return null
  return {
    labels: stats.value.hourlyData.map(h => `${h.hour}:00`),
    datasets: [{
      label: 'Mesatarja (kWh)',
      data: stats.value.hourlyData.map(h => h.averageConsumption),
      backgroundColor: 'rgba(99,102,241,0.7)',
      borderRadius: 4,
    }],
  }
})

const monthlyChartData = computed(() => {
  if (!stats.value?.monthlyData?.length) return null
  return {
    labels: stats.value.monthlyData.map(m => `${m.monthName} ${m.year}`),
    datasets: [{
      label: 'Konsumi mujor (kWh)',
      data: stats.value.monthlyData.map(m => m.totalConsumption),
      backgroundColor: 'rgba(59,130,246,0.7)',
      borderRadius: 4,
    }],
  }
})

const lineOptions = { responsive: true, plugins: { legend: { display: false } }, scales: { y: { beginAtZero: false } } }
const barOptions  = { responsive: true, plugins: { legend: { display: false } } }
</script>
