/**
 * Pinia store for the payment-gateway log.
 *
 * It is the only caller of `useGatewayLogApi`; a refusal becomes the API sentence in `error`.
 */

import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useGatewayLogApi } from '@/composables/apis/useGatewayLogApi'
import { runAction } from '@/lib/runAction'
import type { GatewayLogEntry, GatewayLogFilters } from '@/types/gatewaylog'

/** Lists the gateway log one filtered page at a time. */
export const useGatewayLogStore = defineStore('gateway-log', () => {
  const api = useGatewayLogApi()

  /** Entries of the current page. */
  const entries = ref<GatewayLogEntry[]>([])

  /** Total number of entries matching the filters, across all pages. */
  const totalCount = ref(0)

  /** True while a request is in flight. */
  const loading = ref(false)

  /** Sentence for the last failed request, null when none. */
  const error = ref<string | null>(null)

  /**
   * Loads one filtered page of the log.
   *
   * @param filters - Date range, gateway, debug text and result.
   * @param page - Page number (1-based).
   * @param pageSize - Entries per page.
   * @returns Promise that resolves when the page is loaded.
   */
  const fetchPage = async (filters: GatewayLogFilters, page: number, pageSize: number): Promise<void> => {
    loading.value = true
    const result = await runAction(error, () => api.list(filters, page, pageSize))
    loading.value = false
    if (result.ok) {
      entries.value = result.value.items
      totalCount.value = result.value.totalCount
    }
  }

  return { entries, totalCount, loading, error, fetchPage }
})
