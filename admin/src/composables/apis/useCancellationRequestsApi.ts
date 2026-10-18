/**
 * One function per endpoint on the cancellation-request surface
 * (`/api/admin/cancellation-requests`). No state lives here, no error handling.
 *
 * Callers are the cancellation requests store, never a `.vue` file.
 */

import { buildQuery, useApi } from '@/composables/useApi'
import type { PagedResult } from '@/types/pagedresult'
import type { CancellationRequestItem } from '@/types/service'

/**
 * Endpoint functions for cancellation requests.
 *
 * @returns One typed function per cancellation-request endpoint.
 */
export const useCancellationRequestsApi = () => {
  const { request } = useApi()

  /**
   * Lists cancellation requests.
   *
   * @param page - Page number (1-based).
   * @param pageSize - Items per page.
   * @returns One page of requests.
   */
  const list = (page: number, pageSize: number): Promise<PagedResult<CancellationRequestItem>> =>
    request<PagedResult<CancellationRequestItem>>(`/admin/cancellation-requests${buildQuery({ page, pageSize })}`)

  /**
   * Deletes a cancellation request.
   *
   * @param id - The request id.
   * @returns Nothing.
   */
  const remove = (id: number): Promise<void> =>
    request<void>(`/admin/cancellation-requests/${id}`, { method: 'DELETE' })

  return { list, remove }
}
