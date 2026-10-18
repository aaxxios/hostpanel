/**
 * The anonymous demo-mode endpoint.
 *
 * No state lives here — the demo store owns that. This file only says what the backend offers.
 */

import { useApi } from '@/composables/useApi'
import type { DemoInfo } from '@/types/demo'

/**
 * Endpoint functions for `/api/public/demo`.
 *
 * @returns One typed function per demo endpoint the admin panel uses.
 */
export const useDemoApi = () => {
  const { request } = useApi()

  /**
   * Reads whether this deployment is a demo, when it resets next and the published accounts.
   *
   * @returns The demo description.
   */
  const fetchDemo = (): Promise<DemoInfo> => request<DemoInfo>('/public/demo')

  return { fetchDemo }
}
