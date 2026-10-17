/**
 * Endpoints behind the TLD configs store: TLD pricing CRUD, registrar import and the manual
 * cost-price sync.
 *
 * No state lives here; every function rejects with an `ApiError` carrying the problem+json body.
 */

import { useApi } from '@/composables/useApi'
import type {
  CreateTldConfigPayload,
  TldConfig,
  TldConfigListItem,
  TldImportResult,
  UpdateTldConfigPayload,
} from '@/types/tldconfig'

/**
 * Endpoint functions for `/admin/tld-configs`.
 *
 * @returns One typed function per endpoint.
 */
export const useTldConfigsApi = () => {
  const { request } = useApi()

  /**
   * Lists every TLD config.
   *
   * @returns The list rows.
   */
  const listTldConfigs = (): Promise<TldConfigListItem[]> =>
    request<TldConfigListItem[]>('/admin/tld-configs')

  /**
   * Reads one TLD config.
   *
   * @param id - Config id.
   * @returns The full config.
   */
  const getTldConfig = (id: number): Promise<TldConfig> =>
    request<TldConfig>(`/admin/tld-configs/${id}`)

  /**
   * Creates a TLD config.
   *
   * @param payload - Config data.
   * @returns The new config's id.
   */
  const createTldConfig = (payload: CreateTldConfigPayload): Promise<number> =>
    request<number>('/admin/tld-configs', { method: 'POST', body: JSON.stringify(payload) })

  /**
   * Updates a TLD config.
   *
   * @param id - Config id.
   * @param payload - Updated config data.
   * @returns Nothing; the API answers 204.
   */
  const updateTldConfig = (id: number, payload: UpdateTldConfigPayload): Promise<void> =>
    request<void>(`/admin/tld-configs/${id}`, { method: 'PUT', body: JSON.stringify(payload) })

  /**
   * Deletes a TLD config.
   *
   * @param id - Config id.
   * @returns Nothing; the API answers 204.
   */
  const deleteTldConfig = (id: number): Promise<void> =>
    request<void>(`/admin/tld-configs/${id}`, { method: 'DELETE' })

  /**
   * Imports TLDs from a registrar module.
   *
   * @param module - Registrar module name.
   * @returns Counts of what was imported.
   */
  const importTldConfigs = (module: string): Promise<TldImportResult> =>
    request<TldImportResult>(`/admin/tld-configs/import/${module}`, { method: 'POST' })

  /**
   * Triggers a manual cost-price sync from every provider.
   *
   * @returns Nothing; the API answers 204.
   */
  const syncTldPrices = (): Promise<void> =>
    request<void>('/admin/tld-configs/sync', { method: 'POST' })

  return {
    listTldConfigs,
    getTldConfig,
    createTldConfig,
    updateTldConfig,
    deleteTldConfig,
    importTldConfigs,
    syncTldPrices,
  }
}
