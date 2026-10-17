/**
 * Pinia store for admin cancellation requests.
 *
 * Lazy-loaded: call `fetchAll` before reading `cancellations`. It is the only caller of
 * `useCancellationRequestsApi`; a refusal becomes the API sentence in `error`.
 */

import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useCancellationRequestsApi } from '@/composables/apis/useCancellationRequestsApi'
import { runAction } from '@/lib/runAction'
import type { CancellationRequestItem } from '@/types/service'

/** Lists and deletes service cancellation requests. */
export const useCancellationRequestsStore = defineStore('cancellationRequests', () => {
  const api = useCancellationRequestsApi()

  /** Loaded cancellation requests list. */
  const cancellations = ref<CancellationRequestItem[]>([])

  /** Current page (1-based). */
  const page = ref(1)

  /** Items per page. */
  const pageSize = ref(20)

  /** Total number of matching items. */
  const totalCount = ref(0)

  /** True while a request is in flight. */
  const loading = ref(false)

  /** Sentence for the last failed request, null when none. */
  const error = ref<string | null>(null)

  /**
   * Fetches the current page of cancellation requests.
   *
   * @returns Promise that resolves when data is loaded.
   */
  const fetchAll = async (): Promise<void> => {
    loading.value = true
    const result = await runAction(error, () => api.list(page.value, pageSize.value))
    loading.value = false
    if (result.ok) {
      cancellations.value = result.value.items
      totalCount.value = result.value.totalCount
    }
  }

  /**
   * Deletes a cancellation request, then reloads the current page.
   *
   * @param id - The request id to delete.
   * @returns True when deleted; false when refused (see `error`).
   */
  const deleteRequest = async (id: number): Promise<boolean> => {
    loading.value = true
    const result = await runAction(error, () => api.remove(id))
    loading.value = false
    if (result.ok) {
      await fetchAll()
    }
    return result.ok
  }

  return { cancellations, page, pageSize, totalCount, loading, error, fetchAll, deleteRequest }
})
