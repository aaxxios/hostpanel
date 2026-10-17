/**
 * Endpoints behind the network issues store.
 *
 * No state lives here; every function rejects with an `ApiError` carrying the problem+json body.
 */

import { buildQuery, useApi } from '@/composables/useApi'
import type { NetworkIssue, NetworkIssueFilter, NetworkIssuePayload } from '@/types/networkissue'
import type { PagedResult } from '@/types/pagedresult'

/**
 * Endpoint functions for `/network-issues`.
 *
 * @returns One typed function per endpoint.
 */
export const useNetworkIssuesApi = () => {
  const { request } = useApi()

  /**
   * Lists network issues, one page.
   *
   * @param filter - Page, page size and status.
   * @returns The page and the total count.
   */
  const listNetworkIssues = (filter: NetworkIssueFilter): Promise<PagedResult<NetworkIssue>> =>
    request<PagedResult<NetworkIssue>>(
      `/network-issues${buildQuery({ page: filter.page, pageSize: filter.pageSize, status: filter.status })}`,
    )

  /**
   * Reads one network issue.
   *
   * @param id - Issue id.
   * @returns The issue.
   */
  const getNetworkIssue = (id: number | string): Promise<NetworkIssue> =>
    request<NetworkIssue>(`/network-issues/${id}`)

  /**
   * Creates a network issue.
   *
   * @param payload - The issue's fields.
   * @returns The new issue's id.
   */
  const createNetworkIssue = (payload: NetworkIssuePayload): Promise<number> =>
    request<number>('/network-issues', { method: 'POST', body: JSON.stringify(payload) })

  /**
   * Replaces a network issue.
   *
   * @param id - Issue id.
   * @param payload - The issue's fields.
   * @returns Nothing.
   */
  const updateNetworkIssue = (id: number | string, payload: NetworkIssuePayload): Promise<void> =>
    request<void>(`/network-issues/${id}`, { method: 'PUT', body: JSON.stringify(payload) })

  /**
   * Deletes a network issue.
   *
   * @param id - Issue id.
   * @returns Nothing.
   */
  const deleteNetworkIssue = (id: number | string): Promise<void> =>
    request<void>(`/network-issues/${id}`, { method: 'DELETE' })

  return { listNetworkIssues, getNetworkIssue, createNetworkIssue, updateNetworkIssue, deleteNetworkIssue }
}
