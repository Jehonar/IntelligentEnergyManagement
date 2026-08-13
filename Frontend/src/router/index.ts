import { createRouter, createWebHistory } from 'vue-router'
import DashboardView      from '../views/DashboardView.vue'
import MonitoringView     from '../views/MonitoringView.vue'
import PredictionView     from '../views/PredictionView.vue'
import RecommendationsView from '../views/RecommendationsView.vue'

const routes = [
  { path: '/',               name: 'Dashboard',      component: DashboardView },
  { path: '/monitoring',     name: 'Monitoring',      component: MonitoringView },
  { path: '/prediction',     name: 'Prediction',      component: PredictionView },
  { path: '/recommendations',name: 'Recommendations', component: RecommendationsView },
]

export default createRouter({
  history: createWebHistory(),
  routes,
})
