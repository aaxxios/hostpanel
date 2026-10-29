/**
 * Endpoints behind the network issues store.
 *
 * No state lives here; every function rejects with an `ApiError` carrying the problem+json body.
 */

import { buildQuery, useApi } from '@/composables/useApi'
import type { NetworkIssue, NetworkIssueFilter, NetworkIssuePayload } from '@/types/networkissue'
import { NETWORK_ISSUE_TAB_STATUSES } from '@/types/networkissue'
import type { PaginatedList } from '@/types/paginatedlist'
import { ApiPath } from '@/lib/apiPaths'
import { HttpMethod } from '@/types/http'

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
  const listNetworkIssues = (filter: NetworkIssueFilter): Promise<PaginatedList<NetworkIssue>> =>
    request<PaginatedList<NetworkIssue>>(
      `${ApiPath.NetworkIssues}${buildQuery({
        page: filter.page,
        pageSize: filter.pageSize,
        // A tab can span several statuses, so it travels as a repeated `statuses` parameter.
        statuses: filter.status ? NETWORK_ISSUE_TAB_STATUSES[filter.status] : null,
      })}`,
    )

  /**
   * Reads one network issue.
   *
   * @param id - Issue id.
   * @returns The issue.
   */
  const getNetworkIssue = (id: number | string): Promise<NetworkIssue> =>
    request<NetworkIssue>(`${ApiPath.NetworkIssues}/${id}`)

  /**
   * Creates a network issue.
   *
   * @param payload - The issue's fields.
   * @returns The new issue's id.
   */
  const createNetworkIssue = (payload: NetworkIssuePayload): Promise<number> =>
    request<number>(ApiPath.NetworkIssues, { method: HttpMethod.Post, body: JSON.stringify(payload) })

  /**
   * Replaces a network issue.
   *
   * @param id - Issue id.
   * @param payload - The issue's fields.
   * @returns Nothing.
   */
  const updateNetworkIssue = (id: number | string, payload: NetworkIssuePayload): Promise<void> =>
    request<void>(`${ApiPath.NetworkIssues}/${id}`, { method: HttpMethod.Put, body: JSON.stringify(payload) })

  /**
   * Deletes a network issue.
   *
   * @param id - Issue id.
   * @returns Nothing.
   */
  const deleteNetworkIssue = (id: number | string): Promise<void> =>
    request<void>(`${ApiPath.NetworkIssues}/${id}`, { method: HttpMethod.Delete })

  return { listNetworkIssues, getNetworkIssue, createNetworkIssue, updateNetworkIssue, deleteNetworkIssue }
}
