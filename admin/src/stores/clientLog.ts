/**
 * Pinia store for the activity log of one client profile.
 *
 * Lazy-loaded: call `fetchLogs` before reading `logs`. It reads through
 * `useClientHistoryApi`; a refusal becomes the API sentence in `error`.
 */

import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useClientHistoryApi } from '@/composables/apis/useClientHistoryApi'
import { runAction } from '@/lib/runAction'
import type { ActivityLog, ActivityLogFilters } from '@/types/activitylog'

/** Loads the activity log of a client. */
export const useClientLogStore = defineStore('clientLog', () => {
  const api = useClientHistoryApi()

  /** Fetched activity log entries. */
  const logs = ref<ActivityLog[]>([])

  /** Total number of matching entries. */
  const totalCount = ref(0)

  /** True while an API request is in flight. */
  const loading = ref(false)

  /** Sentence for the last failed load, null when none. */
  const error = ref<string | null>(null)

  /**
   * Fetches paginated, filtered activity logs for a client.
   *
   * @param clientId - The client primary key.
   * @param page - 1-based page number.
   * @param pageSize - Entries per page.
   * @param filters - Optional filter parameters.
   * @returns Promise that resolves when logs are loaded.
   */
  const fetchLogs = async (
    clientId: number,
    page: number,
    pageSize: number,
    filters?: ActivityLogFilters,
  ): Promise<void> => {
    loading.value = true
    const result = await runAction(error, () => api.activityLogs(clientId, page, pageSize, filters))
    loading.value = false
    if (result.ok) {
      logs.value = result.value.items
      totalCount.value = result.value.totalCount
    }
  }

  /**
   * Resets the store to its initial state.
   *
   * @returns Nothing.
   */
  const reset = (): void => {
    logs.value = []
    totalCount.value = 0
    loading.value = false
    error.value = null
  }

  return { logs, totalCount, loading, error, fetchLogs, reset }
})
