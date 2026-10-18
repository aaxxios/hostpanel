/**
 * Endpoints behind the services store: the service list, one service's detail, the save and the
 * provisioning commands that act on the hosting server.
 *
 * No state lives here; every function rejects with an `ApiError` carrying the problem+json body.
 */

import { buildQuery, useApi } from '@/composables/useApi'
import type { PagedResult } from '@/types/pagedresult'
import type {
  ProvisioningReasonPayload,
  ServiceDetail,
  ServiceFilter,
  ServiceListItem,
  UpdateServicePayload,
} from '@/types/service'

/**
 * Endpoint functions for `/services` and `/provisioning`.
 *
 * @returns One typed function per endpoint.
 */
export const useServicesApi = () => {
  const { request } = useApi()

  /**
   * Lists services, one page.
   *
   * @param filter - Page, page size and owning client.
   * @returns The page and the total count.
   */
  const listServices = (filter: ServiceFilter): Promise<PagedResult<ServiceListItem>> =>
    request<PagedResult<ServiceListItem>>(
      `/services${buildQuery({ clientId: filter.clientId, page: filter.page, pageSize: filter.pageSize })}`,
    )

  /**
   * Reads one service.
   *
   * @param id - Service id.
   * @returns The service detail.
   */
  const getService = (id: number | string): Promise<ServiceDetail> => request<ServiceDetail>(`/services/${id}`)

  /**
   * Saves the editable fields of a service.
   *
   * @param id - Service id.
   * @param payload - The editable fields.
   * @returns Nothing.
   */
  const updateService = (id: number | string, payload: UpdateServicePayload): Promise<void> =>
    request<void>(`/services/${id}`, { method: 'PUT', body: JSON.stringify(payload) })

  /**
   * Asks the provisioning server for a one-time control-panel login URL.
   *
   * @param id - Service id.
   * @returns The URL.
   */
  const getPanelLoginUrl = (id: number | string): Promise<{ url: string }> =>
    request<{ url: string }>(`/provisioning/${id}/cpanel-sso`)

  /**
   * Provisions a pending service.
   *
   * @param id - Service id.
   * @returns Nothing.
   */
  const provisionService = (id: number | string): Promise<void> =>
    request<void>(`/provisioning/${id}/provision`, { method: 'POST' })

  /**
   * Changes the hosting account password.
   *
   * @param id - Service id.
   * @param newPassword - The new password.
   * @returns Nothing.
   */
  const changePassword = (id: number | string, newPassword: string): Promise<void> =>
    request<void>(`/provisioning/${id}/change-password`, { method: 'POST', body: JSON.stringify({ newPassword }) })

  /**
   * Changes the hosting package.
   *
   * @param id - Service id.
   * @param newPackage - The new package name.
   * @returns Nothing.
   */
  const changePackage = (id: number | string, newPackage: string): Promise<void> =>
    request<void>(`/provisioning/${id}/change-package`, { method: 'POST', body: JSON.stringify({ newPackage }) })

  /**
   * Suspends a service.
   *
   * @param id - Service id.
   * @param payload - The reason recorded with the suspension.
   * @returns Nothing.
   */
  const suspendService = (id: number | string, payload: ProvisioningReasonPayload): Promise<void> =>
    request<void>(`/provisioning/${id}/suspend`, { method: 'POST', body: JSON.stringify(payload) })

  /**
   * Unsuspends a service.
   *
   * @param id - Service id.
   * @returns Nothing.
   */
  const unsuspendService = (id: number | string): Promise<void> =>
    request<void>(`/provisioning/${id}/unsuspend`, { method: 'POST' })

  /**
   * Terminates a service.
   *
   * @param id - Service id.
   * @param payload - The reason recorded with the termination.
   * @returns Nothing.
   */
  const terminateService = (id: number | string, payload: ProvisioningReasonPayload): Promise<void> =>
    request<void>(`/provisioning/${id}/terminate`, { method: 'POST', body: JSON.stringify(payload) })

  return {
    listServices, getService, updateService, getPanelLoginUrl,
    provisionService, changePassword, changePackage, suspendService, unsuspendService, terminateService,
  }
}
