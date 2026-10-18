/**
 * Pinia store for the e-mail log of one client profile.
 *
 * Lazy-loaded: call `fetchClientEmails` before reading `emails`. It reads through
 * `useClientHistoryApi`; a refusal becomes the API sentence in `error`.
 */

import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useClientHistoryApi } from '@/composables/apis/useClientHistoryApi'
import { runAction } from '@/lib/runAction'
import type { EmailLog } from '@/types/emaillog'

/** Loads the e-mails sent to a client. */
export const useClientEmailStore = defineStore('clientEmail', () => {
  const api = useClientHistoryApi()

  /** Fetched e-mail log entries for the current client. */
  const emails = ref<EmailLog[]>([])

  /** Total number of matching e-mail log entries. */
  const totalCount = ref(0)

  /** True while an API request is in flight. */
  const loading = ref(false)

  /** Sentence for the last failed load, null when none. */
  const error = ref<string | null>(null)

  /**
   * Fetches a paginated list of e-mail logs sent to a client.
   *
   * @param clientId - The client primary key.
   * @param page - 1-based page number.
   * @param pageSize - Number of entries per page.
   * @returns Promise that resolves when e-mails are loaded.
   */
  const fetchClientEmails = async (clientId: number, page: number, pageSize: number): Promise<void> => {
    loading.value = true
    const result = await runAction(error, () => api.emailLogs(clientId, page, pageSize))
    loading.value = false
    if (result.ok) {
      emails.value = result.value.items
      totalCount.value = result.value.totalCount
    }
  }

  /**
   * Resets the store to its initial state.
   *
   * @returns Nothing.
   */
  const reset = (): void => {
    emails.value = []
    totalCount.value = 0
    loading.value = false
    error.value = null
  }

  return { emails, totalCount, loading, error, fetchClientEmails, reset }
})
