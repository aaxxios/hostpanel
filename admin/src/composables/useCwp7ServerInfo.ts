/**
 * Live CWP7 server status, read from the server-info endpoint.
 *
 * Holds the loading and error state so a page only calls `fetch()` and renders. The request goes
 * through `useIntegrationsApi`; a refusal is kept as the API's own sentence, never thrown.
 */

import { ref } from 'vue'
import { useIntegrationsApi } from '@/composables/apis/useIntegrationsApi'
import { runAction } from '@/lib/runAction'
import type { Cwp7ServerInfoDto } from '@/types/cwp7'

/**
 * Fetches and manages live CWP7 server status from the server-info endpoint.
 *
 * Handles loading and error states automatically. Call `fetch()` to load data.
 *
 * @returns Reactive server info, loading flag, error sentence, and fetch action.
 */
export const useCwp7ServerInfo = () => {
  const api = useIntegrationsApi()

  /** CWP7 server info data, null until first successful fetch. */
  const info = ref<Cwp7ServerInfoDto | null>(null)

  /** True while a fetch is in progress. */
  const loading = ref(false)

  /** Sentence for the last failed fetch, null when none. */
  const error = ref<string | null>(null)

  /**
   * Fetches live CWP7 server status from the backend.
   *
   * On failure sets `error` and leaves `info` as its previous value. Never throws.
   *
   * @returns Resolves when the fetch is over.
   */
  const fetch = async (): Promise<void> => {
    loading.value = true
    const result = await runAction(error, api.getCwp7ServerInfo)
    loading.value = false
    if (result.ok) {
      info.value = result.value
    }
  }

  return { info, loading, error, fetch }
}
