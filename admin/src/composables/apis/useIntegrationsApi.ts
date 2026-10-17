/**
 * Endpoints behind the integrations store and the CWP server-info composables: the integration
 * list, per-integration config, the connection test and the live server status of the two
 * control-panel integrations.
 *
 * No state lives here; every function rejects with an `ApiError` carrying the problem+json body.
 */

import { useApi } from '@/composables/useApi'
import type {
  IntegrationConfigPayload,
  IntegrationDetailDto,
  IntegrationDto,
  IntegrationTestResult,
} from '@/types/integration'
import type { CwpServerInfoDto } from '@/types/cwp'
import type { Cwp7ServerInfoDto } from '@/types/cwp7'

/**
 * Endpoint functions for `/admin/integrations`.
 *
 * @returns One typed function per endpoint.
 */
export const useIntegrationsApi = () => {
  const { request } = useApi()

  /**
   * Lists the integrations.
   *
   * @returns The summary rows, with the backend's category strings untouched.
   */
  const listIntegrations = (): Promise<IntegrationDto[]> =>
    request<IntegrationDto[]>('/admin/integrations')

  /**
   * Reads one integration's config.
   *
   * @param slug - Integration slug.
   * @returns The detail with its field definitions and values.
   */
  const getIntegration = (slug: string): Promise<IntegrationDetailDto> =>
    request<IntegrationDetailDto>(`/admin/integrations/${slug}`)

  /**
   * Saves an integration's config.
   *
   * @param slug - Integration slug.
   * @param payload - Enabled state and config values.
   * @returns Nothing; the API answers 204 No Content.
   */
  const saveIntegration = (slug: string, payload: IntegrationConfigPayload): Promise<void> =>
    request<void>(`/admin/integrations/${slug}`, { method: 'PUT', body: JSON.stringify(payload) })

  /**
   * Runs a live connection test.
   *
   * @param slug - Integration slug.
   * @returns The probe result.
   */
  const testIntegration = (slug: string): Promise<IntegrationTestResult> =>
    request<IntegrationTestResult>(`/admin/integrations/${slug}/test`, { method: 'POST' })

  /**
   * Reads the live status of the connected CWP server.
   *
   * @returns The server status.
   */
  const getCwpServerInfo = (): Promise<CwpServerInfoDto> =>
    request<CwpServerInfoDto>('/admin/integrations/cwp/server-info')

  /**
   * Reads the live status of the connected CWP7 server.
   *
   * @returns The server status.
   */
  const getCwp7ServerInfo = (): Promise<Cwp7ServerInfoDto> =>
    request<Cwp7ServerInfoDto>('/admin/integrations/cwp7/server-info')

  return {
    listIntegrations,
    getIntegration,
    saveIntegration,
    testIntegration,
    getCwpServerInfo,
    getCwp7ServerInfo,
  }
}
