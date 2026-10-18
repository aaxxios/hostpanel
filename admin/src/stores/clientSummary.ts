/**
 * Pinia store for the aggregated summary of one client profile.
 *
 * Lazy-loaded: call `fetchAll` before reading any state. The summary figures are critical and
 * surface a refusal in `error`; the three recent-activity tables are not, so a failure there
 * leaves the table empty without an error. It reads through `useClientsApi` and
 * `useQuotesApi`, never a `.vue` file.
 */

import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useClientsApi } from '@/composables/apis/useClientsApi'
import { useQuotesApi } from '@/composables/apis/useQuotesApi'
import { runAction } from '@/lib/runAction'
import type { ClientSummaryData } from '@/types/client'
import type { DomainRegistration } from '@/types/domain'
import type { QuoteListItem } from '@/types/quote'
import type { ServiceListItem } from '@/types/service'

/** Number of rows the recent tables show. */
const RECENT_COUNT = 5

/** Loads the summary figures and recent tables of a client profile. */
export const useClientSummaryStore = defineStore('clientSummary', () => {
  const clientsApi = useClientsApi()
  const quotesApi = useQuotesApi()

  /** Aggregated summary data from the backend. */
  const summary = ref<ClientSummaryData | null>(null)

  /** Recent services for the bottom table. */
  const services = ref<ServiceListItem[]>([])

  /** Recent domains for the bottom table. */
  const domains = ref<DomainRegistration[]>([])

  /** Recent quotes for the bottom table. */
  const quotes = ref<QuoteListItem[]>([])

  /** True while any request is in flight. */
  const loading = ref(false)

  /** Sentence for the last failed summary load, null when none. */
  const error = ref<string | null>(null)

  /** Throwaway error ref: the recent tables are non-critical, so their failure is silent. */
  const quiet = ref<string | null>(null)

  /**
   * Fetches the aggregated summary data for a client.
   *
   * @param clientId - The client primary key.
   * @returns Promise that resolves when the summary is loaded.
   */
  const fetchSummary = async (clientId: number): Promise<void> => {
    const result = await runAction(error, () => clientsApi.summary(clientId))
    if (result.ok) {
      summary.value = result.value
    }
  }

  /**
   * Fetches the most recent services for a client.
   *
   * @param clientId - The client primary key.
   * @returns Promise that resolves when services are loaded.
   */
  const fetchRecentServices = async (clientId: number): Promise<void> => {
    const result = await runAction(quiet, () => clientsApi.recentServices(clientId, RECENT_COUNT))
    if (result.ok) {
      services.value = result.value.items
    }
  }

  /**
   * Fetches the most recent domains for a client.
   *
   * @param clientId - The client primary key.
   * @returns Promise that resolves when domains are loaded.
   */
  const fetchRecentDomains = async (clientId: number): Promise<void> => {
    const result = await runAction(quiet, () => clientsApi.recentDomains(clientId, RECENT_COUNT))
    if (result.ok) {
      domains.value = result.value.items
    }
  }

  /**
   * Fetches the most recent quotes for a client.
   *
   * @param clientId - The client primary key.
   * @returns Promise that resolves when quotes are loaded.
   */
  const fetchRecentQuotes = async (clientId: number): Promise<void> => {
    const result = await runAction(quiet, () => quotesApi.listByClient(clientId, 1, RECENT_COUNT))
    if (result.ok) {
      quotes.value = result.value.items
    }
  }

  /**
   * Fetches all summary data in parallel: summary stats, services, domains and quotes.
   *
   * @param clientId - The client primary key.
   * @returns Promise that resolves when all data is loaded.
   */
  const fetchAll = async (clientId: number): Promise<void> => {
    loading.value = true
    error.value = null
    await Promise.all([
      fetchSummary(clientId),
      fetchRecentServices(clientId),
      fetchRecentDomains(clientId),
      fetchRecentQuotes(clientId),
    ])
    loading.value = false
  }

  /**
   * Resets the store to its initial state.
   *
   * @returns Nothing.
   */
  const reset = (): void => {
    summary.value = null
    services.value = []
    domains.value = []
    quotes.value = []
    loading.value = false
    error.value = null
  }

  return { summary, services, domains, quotes, loading, error, fetchAll, reset }
})
