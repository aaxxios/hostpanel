import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useDashboardApi } from '@/composables/apis/useDashboardApi'
import { runAction } from '@/lib/runAction'
import type { DashboardStats } from '@/types/dashboardstats'

/**
 * Pinia store for admin dashboard statistics.
 *
 * Fetches and caches stats through `useDashboardApi`; a refusal shows the API's own sentence.
 */
export const useDashboardStore = defineStore('dashboard', () => {
  const api = useDashboardApi()

  /** Dashboard statistics, null until loaded. */
  const stats = ref<DashboardStats | null>(null)

  /** True while stats are being fetched. */
  const loading = ref(false)

  /** Sentence for the last failed call, null when no error. */
  const error = ref<string | null>(null)

  /**
   * Loads dashboard statistics from the backend.
   *
   * @returns Promise that resolves when the attempt is over (see `error`).
   */
  const fetchStats = async (): Promise<void> => {
    loading.value = true
    const result = await runAction(error, () => api.fetchStats())
    if (result.ok) {
      stats.value = result.value
    }
    loading.value = false
  }

  return { stats, loading, error, fetchStats }
})
