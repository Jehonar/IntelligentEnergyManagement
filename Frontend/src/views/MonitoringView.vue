<template>
  <div class="p-8">
    <div class="mb-8">
      <h1 class="text-2xl font-bold text-gray-900">Monitorimi i energjisë</h1>
      <p class="text-gray-500 mt-1">Shfletoni dhe filtroni leximet e energjisë</p>
    </div>

    <!-- Filters -->
    <div class="card mb-6">
      <div class="grid grid-cols-1 sm:grid-cols-4 gap-4">
        <div>
          <label class="block text-xs font-medium text-gray-500 mb-1">Nga</label>
          <input v-model="filters.from" type="date" class="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-500 mb-1">Deri</label>
          <input v-model="filters.to" type="date" class="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-500 mb-1">Pajisja</label>
          <select v-model="filters.device" class="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
            <option value="">Të gjitha pajisjet</option>
            <option v-for="d in devices" :key="d" :value="d">{{ d }}</option>
          </select>
        </div>
        <div class="flex items-end">
          <button @click="load" class="btn-primary w-full">Apliko filtrat</button>
        </div>
      </div>
    </div>

    <!-- Summary cards -->
    <div class="grid grid-cols-2 md:grid-cols-5 gap-4 mb-6" v-if="readings.length">
      <div class="card text-center">
        <p class="text-xs text-gray-400">Totali</p>
        <p class="text-xl font-bold text-blue-600">{{ totalConsumption }}</p>
        <p class="text-xs text-gray-400">kWh</p>
      </div>
      <div class="card text-center">
        <p class="text-xs text-gray-400">Mesatarja</p>
        <p class="text-xl font-bold text-indigo-600">{{ avgConsumption }}</p>
        <p class="text-xs text-gray-400">kWh</p>
      </div>
      <div class="card text-center">
        <p class="text-xs text-gray-400">Maksimumi</p>
        <p class="text-xl font-bold text-red-600">{{ maxConsumption }}</p>
        <p class="text-xs text-gray-400">kWh</p>
      </div>
      <div class="card text-center">
        <p class="text-xs text-gray-400">Minimumi</p>
        <p class="text-xl font-bold text-green-600">{{ minConsumption }}</p>
        <p class="text-xs text-gray-400">kWh</p>
      </div>
      <div class="card text-center">
        <p class="text-xs text-gray-400">Regjistrime</p>
        <p class="text-xl font-bold text-gray-700">{{ readings.length }}</p>
        <p class="text-xs text-gray-400">lexime</p>
      </div>
    </div>

    <!-- Loading / Error -->
    <div v-if="loading" class="flex items-center justify-center h-32">
      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
    </div>
    <div v-else-if="error" class="card border-red-200 bg-red-50 text-red-700 text-sm">{{ error }}</div>

    <!-- Table -->
    <div v-else class="card overflow-hidden p-0">
      <div class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead class="bg-gray-50 border-b border-gray-100">
            <tr>
              <th class="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Data</th>
              <th class="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Ora</th>
              <th class="text-left px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Pajisja</th>
              <th class="text-right px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Konsumi (kWh)</th>
              <th class="text-right px-4 py-3 text-xs font-semibold text-gray-500 uppercase tracking-wide">Temperatura (°C)</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-50">
            <tr v-for="r in readings" :key="r.id" class="hover:bg-gray-50 transition-colors">
              <td class="px-4 py-2.5 text-gray-700">{{ r.readingDate }}</td>
              <td class="px-4 py-2.5 text-gray-500">{{ r.readingHour }}:00</td>
              <td class="px-4 py-2.5">
                <span class="px-2 py-0.5 rounded text-xs font-medium bg-gray-100 text-gray-700">
                  {{ r.deviceName }}
                </span>
              </td>
              <td class="px-4 py-2.5 text-right font-mono font-semibold text-blue-700">
                {{ Number(r.energyConsumption).toFixed(4) }}
              </td>
              <td class="px-4 py-2.5 text-right text-gray-500">
                {{ r.temperature !== null ? Number(r.temperature).toFixed(1) : '–' }}
              </td>
            </tr>
          </tbody>
        </table>
        <p v-if="!readings.length" class="text-center py-12 text-gray-400">Nuk u gjetën lexime për filtrat e zgjedhur.</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { getReadings, getDevices, type EnergyReading } from '../api'

const readings = ref<EnergyReading[]>([])
const devices  = ref<string[]>([])
const loading  = ref(false)
const error    = ref('')

const filters = ref({ from: '', to: '', device: '' })

const totalConsumption = computed(() => readings.value.reduce((s, r) => s + Number(r.energyConsumption), 0).toFixed(2))
const avgConsumption   = computed(() => readings.value.length ? (readings.value.reduce((s, r) => s + Number(r.energyConsumption), 0) / readings.value.length).toFixed(4) : '0')
const maxConsumption   = computed(() => readings.value.length ? Math.max(...readings.value.map(r => Number(r.energyConsumption))).toFixed(4) : '0')
const minConsumption   = computed(() => readings.value.length ? Math.min(...readings.value.map(r => Number(r.energyConsumption))).toFixed(4) : '0')

async function load() {
  loading.value = true
  error.value   = ''
  try {
    readings.value = await getReadings({
      from:   filters.value.from   || undefined,
      to:     filters.value.to     || undefined,
      device: filters.value.device || undefined,
    })
  } catch (e: any) {
    error.value = e?.message ?? 'Ngarkimi i leximeve dështoi'
  } finally {
    loading.value = false
  }
}

onMounted(async () => {
  devices.value = await getDevices().catch(() => [])
  await load()
})
</script>
