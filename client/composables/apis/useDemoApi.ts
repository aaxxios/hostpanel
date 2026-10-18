/**
 * Endpoint for the demo-mode status.
 *
 * Returns a plain `Promise` through `apiFetch`, so `stores/demo.ts` can call it from a store
 * action. Nothing here is cached; the store keeps the state.
 *
 * @module composables/apis/useDemoApi
 */

import { apiFetch } from '~/composables/useApi'
import type { DemoStatus } from '~/types/demostatus'

/**
 * The demo-mode surface, one function per endpoint.
 *
 * @returns The demo endpoint functions.
 */
export const useDemoApi = () => {
  /**
   * Reads whether this deployment is a demo, its next reset and its published accounts.
   *
   * @returns The demo status.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchDemo = (): Promise<DemoStatus> => apiFetch<DemoStatus>('/api/portal/demo')

  return { fetchDemo }
}
