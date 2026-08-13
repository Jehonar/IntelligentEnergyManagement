import axios from 'axios'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5000',
  timeout: 30_000,
})

// ── Types ──────────────────────────────────────────────────────────────────

export interface EnergyReading {
  id: number
  readingDate: string
  readingHour: number
  energyConsumption: number
  temperature: number | null
  deviceName: string
}

export interface EnergyStatistics {
  totalConsumption: number
  averageDailyConsumption: number
  latestConsumption: number
  highestConsumption: number
  lowestConsumption: number
  dailyData: { date: string; totalConsumption: number }[]
  hourlyData: { hour: number; averageConsumption: number }[]
  monthlyData: { year: number; month: number; monthName: string; totalConsumption: number }[]
}

export interface PredictionRequest {
  hour: number
  dayOfWeek: number
  month: number
  temperature: number
  previousConsumption: number
}

export interface PredictionResponse {
  id: number
  predictionDate: string
  predictionHour: number
  predictedConsumption: number
  createdAt: string
}

export interface Recommendation {
  id: number
  predictionId: number | null
  message: string
  recommendationType: string
  createdAt: string
}

// ── Energy endpoints ───────────────────────────────────────────────────────

export const getStatistics = () =>
  api.get<EnergyStatistics>('/api/energy/statistics').then(r => r.data)

export const getReadings = (params?: {
  from?: string
  to?: string
  device?: string
}) => api.get<EnergyReading[]>('/api/energy', { params }).then(r => r.data)

export const getDailyReadings = (days = 30) =>
  api.get<{ date: string; totalConsumption: number }[]>('/api/energy/daily', { params: { days } }).then(r => r.data)

export const getDevices = () =>
  api.get<string[]>('/api/energy/devices').then(r => r.data)

// ── Prediction endpoints ───────────────────────────────────────────────────

export const requestPrediction = (body: PredictionRequest) =>
  api.post<PredictionResponse>('/api/prediction', body).then(r => r.data)

export const getPredictionHistory = (limit = 20) =>
  api.get<PredictionResponse[]>('/api/prediction/history', { params: { limit } }).then(r => r.data)

// ── Recommendation endpoints ───────────────────────────────────────────────

export const generateRecommendation = (predictedConsumption: number, predictionId?: number) =>
  api.post<Recommendation>('/api/recommendations', { predictedConsumption, predictionId }).then(r => r.data)

export const getRecommendations = (limit = 10) =>
  api.get<Recommendation[]>('/api/recommendations', { params: { limit } }).then(r => r.data)
