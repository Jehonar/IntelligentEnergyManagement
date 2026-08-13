<template>
  <div class="p-8">
    <div class="mb-8">
      <h1 class="text-2xl font-bold text-gray-900">Parashikimi AI i energjisë</h1>
      <p class="text-gray-500 mt-1">Përdorni modelin Random Forest për të parashikuar konsumin e ardhshëm</p>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
      <!-- Form -->
      <div class="card">
        <h2 class="text-base font-semibold text-gray-700 mb-5">Parametrat e parashikimit</h2>
        <form @submit.prevent="predict" class="space-y-4">
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-xs font-medium text-gray-500 mb-1">Ora</label>
              <input v-model.number="form.hour" type="number" min="0" max="23" required
                class="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
            </div>
            <div>
              <label class="block text-xs font-medium text-gray-500 mb-1">Dita e javës</label>
              <select v-model.number="form.dayOfWeek"
                class="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                <option :value="0">E hënë</option>
                <option :value="1">E martë</option>
                <option :value="2">E mërkurë</option>
                <option :value="3">E enjte</option>
                <option :value="4">E premte</option>
                <option :value="5">E shtunë</option>
                <option :value="6">E diel</option>
              </select>
            </div>
            <div>
              <label class="block text-xs font-medium text-gray-500 mb-1">Muaji</label>
              <select v-model.number="form.month"
                class="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                <option v-for="(m, i) in months" :key="i" :value="i + 1">{{ m }}</option>
              </select>
            </div>
            <div>
              <label class="block text-xs font-medium text-gray-500 mb-1">Temperatura (°C)</label>
              <input v-model.number="form.temperature" type="number" step="0.1" required
                class="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
            </div>
          </div>
          <div>
            <label class="block text-xs font-medium text-gray-500 mb-1">Konsumi i mëparshëm (kWh)</label>
            <input v-model.number="form.previousConsumption" type="number" step="0.01" min="0" required
              class="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
          </div>
          <button type="submit" class="btn-primary w-full" :disabled="loading">
            {{ loading ? 'Duke parashikuar…' : 'Parashikimi sipas AI' }}
          </button>
        </form>
      </div>

      <!-- Result -->
      <div class="flex flex-col gap-6">
        <div class="card" v-if="result">
          <p class="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-1">Konsumi i parashikuar i energjisë</p>
          <p class="text-gray-500 text-sm mb-3">
            {{ result.predictionDate }} në {{ result.predictionHour }}:00
          </p>
          <div class="flex items-baseline gap-2 mb-4">
            <span class="text-5xl font-extrabold text-blue-600">{{ result.predictedConsumption.toFixed(2) }}</span>
            <span class="text-xl text-gray-400">kWh</span>
          </div>
          <div v-if="recommendation" class="rounded-lg p-4 mt-2" :class="recBgClass">
            <p class="text-sm font-semibold mb-1" :class="recTextClass">{{ recTypeLabel(recommendation.recommendationType) }}</p>
            <p class="text-sm text-gray-700">{{ recommendation.message }}</p>
          </div>
          <button v-if="!recommendation" @click="getRecommendation" class="btn-secondary mt-3 text-sm">
            Merr rekomandimin
          </button>
        </div>
        <div v-else class="card flex items-center justify-center h-48 text-gray-400">
          <div class="text-center">
            <CpuChipIcon class="w-12 h-12 mx-auto mb-2 text-gray-300" />
            <p>Plotësoni formularin dhe klikoni Parashikimi sipas AI</p>
          </div>
        </div>

        <div class="card" v-if="comparisonChartData">
          <h2 class="text-sm font-semibold text-gray-700 mb-3">Konsumi aktual vs i parashikuar</h2>
          <Bar :data="comparisonChartData" :options="chartOptions" />
        </div>
      </div>
    </div>

    <!-- History -->
    <div class="card mt-8" v-if="history.length">
      <h2 class="text-base font-semibold text-gray-700 mb-4">Historiku i parashikimeve</h2>
      <div class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead class="bg-gray-50 border-b border-gray-100">
            <tr>
              <th class="text-left px-4 py-2 text-xs font-semibold text-gray-400 uppercase">Data</th>
              <th class="text-left px-4 py-2 text-xs font-semibold text-gray-400 uppercase">Ora</th>
              <th class="text-right px-4 py-2 text-xs font-semibold text-gray-400 uppercase">Parashikuar (kWh)</th>
              <th class="text-right px-4 py-2 text-xs font-semibold text-gray-400 uppercase">Krijuar</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-50">
            <tr v-for="h in history" :key="h.id" class="hover:bg-gray-50">
              <td class="px-4 py-2 text-gray-700">{{ h.predictionDate }}</td>
              <td class="px-4 py-2 text-gray-500">{{ h.predictionHour }}:00</td>
              <td class="px-4 py-2 text-right font-mono font-semibold text-blue-700">{{ h.predictedConsumption.toFixed(4) }}</td>
              <td class="px-4 py-2 text-right text-gray-400 text-xs">{{ new Date(h.createdAt).toLocaleString('sq-AL') }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { Bar } from 'vue-chartjs'
import { CpuChipIcon } from '@heroicons/vue/24/outline'
import {
  Chart as ChartJS, CategoryScale, LinearScale,
  BarElement, Title, Tooltip, Legend
} from 'chart.js'
import {
  requestPrediction, getPredictionHistory, generateRecommendation,
  getStatistics,
  type PredictionResponse, type Recommendation
} from '../api'

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend)

const now = new Date()

const form = ref({
  hour:                now.getHours(),
  dayOfWeek:           now.getDay() === 0 ? 6 : now.getDay() - 1,
  month:               now.getMonth() + 1,
  temperature:         20.0,
  previousConsumption: 10.0,
})

const loading      = ref(false)
const result       = ref<PredictionResponse | null>(null)
const recommendation = ref<Recommendation | null>(null)
const history      = ref<PredictionResponse[]>([])
const avgConsumption = ref(0)

const months = [
  'Janar', 'Shkurt', 'Mars', 'Prill', 'Maj', 'Qershor',
  'Korrik', 'Gusht', 'Shtator', 'Tetor', 'Nëntor', 'Dhjetor',
]

const recTypeLabels: Record<string, string> = {
  HIGH: 'KONSUM I LARTË',
  MODERATE: 'KONSUM MODERAT',
  NORMAL: 'KONSUM NORMAL',
  LOW: 'KONSUM I ULËT',
}

function recTypeLabel(type: string) {
  return recTypeLabels[type] ?? type
}

async function predict() {
  loading.value = true
  recommendation.value = null
  try {
    result.value = await requestPrediction(form.value)
    history.value = await getPredictionHistory(10)
  } catch (e: any) {
    alert('Parashikimi dështoi: ' + (e?.message ?? 'Gabim i panjohur'))
  } finally {
    loading.value = false
  }
}

async function getRecommendation() {
  if (!result.value) return
  recommendation.value = await generateRecommendation(result.value.predictedConsumption, result.value.id)
}

const recBgClass = computed(() => {
  const t = recommendation.value?.recommendationType
  if (t === 'HIGH')     return 'bg-red-50 border border-red-200'
  if (t === 'MODERATE') return 'bg-yellow-50 border border-yellow-200'
  if (t === 'LOW')      return 'bg-blue-50 border border-blue-200'
  return 'bg-green-50 border border-green-200'
})

const recTextClass = computed(() => {
  const t = recommendation.value?.recommendationType
  if (t === 'HIGH')     return 'text-red-700'
  if (t === 'MODERATE') return 'text-yellow-700'
  if (t === 'LOW')      return 'text-blue-700'
  return 'text-green-700'
})

const comparisonChartData = computed(() => {
  if (!result.value || !avgConsumption.value) return null
  return {
    labels: ['Mesatarja e konsumit', 'Parashikimi AI'],
    datasets: [{
      label: 'kWh',
      data: [avgConsumption.value, result.value.predictedConsumption],
      backgroundColor: ['rgba(99,102,241,0.7)', 'rgba(59,130,246,0.7)'],
      borderRadius: 6,
    }],
  }
})

const chartOptions = {
  responsive: true,
  plugins: { legend: { display: false } },
  scales: { y: { beginAtZero: false } },
}

onMounted(async () => {
  history.value = await getPredictionHistory(10).catch(() => [])
  const stats = await getStatistics().catch(() => null)
  if (stats) avgConsumption.value = stats.averageDailyConsumption
})
</script>
