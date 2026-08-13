<template>
  <div class="min-h-screen flex">
    <!-- Sidebar -->
    <aside class="sidebar w-64 text-white flex flex-col shadow-xl relative overflow-hidden">
      <div class="sidebar-overlay absolute inset-0 pointer-events-none" />

      <div class="relative z-10 px-6 py-6 border-b border-white/10">
        <div class="flex items-center gap-3">
          <div class="logo-icon flex items-center justify-center text-sm font-bold bg-blue-500">˗ˏˋ⚡︎ˎˊ˗</div>
          <div>
            <p class="font-bold text-sm leading-tight">Intelligent Energy</p>
            <p class="text-blue-200 text-xs">Management System</p>
          </div>
        </div>
      </div>

      <nav class="relative z-10 flex-1 px-3 py-4 space-y-1">
        <RouterLink
          v-for="item in navItems"
          :key="item.to"
          :to="item.to"
          class="flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors duration-150"
          :class="[$route.path === item.to
            ? 'bg-white/20 text-white shadow-sm backdrop-blur-sm'
            : 'text-blue-100 hover:bg-white/10 hover:text-white']"
        >
          <component :is="item.icon" class="w-5 h-5 shrink-0 opacity-90" />
          {{ item.label }}
        </RouterLink>
      </nav>

      <div class="relative z-10 px-6 py-4 border-t border-white/10 text-blue-200 text-xs">
        Projekt Universitar · 2026
      </div>
    </aside>

    <!-- Main content -->
    <main class="flex-1 overflow-auto">
      <RouterView />
    </main>
  </div>
</template>

<script setup lang="ts">
import { RouterLink, RouterView, useRoute } from 'vue-router'
import {
  HomeIcon,
  ChartBarIcon,
  CpuChipIcon,
  LightBulbIcon,
} from '@heroicons/vue/24/outline'

const $route = useRoute()

const navItems = [
  { to: '/',                icon: HomeIcon,       label: 'Ballina' },
  { to: '/monitoring',      icon: ChartBarIcon,   label: 'Monitorimi i energjisë' },
  { to: '/prediction',      icon: CpuChipIcon,    label: 'Parashikimi AI' },
  { to: '/recommendations', icon: LightBulbIcon,  label: 'Rekomandimet' },
]
</script>

<style scoped>
.sidebar {
  background-image: url('/sidebar-bg.png');
  background-size: cover;
  background-position: center bottom;
  background-repeat: no-repeat;
}

.sidebar-overlay {
  background: linear-gradient(
    180deg,
    rgba(15, 23, 42, 0.93) 0%,
    rgba(30, 58, 138, 0.88) 45%,
    rgba(37, 99, 235, 0.82) 100%
  );
}

.logo-icon {
  letter-spacing: -0.02em;
  white-space: nowrap;
}
</style>
