/**
 * Endpoints behind the orders store: the order list, one order's detail and its lifecycle commands.
 *
 * No state lives here; every function rejects with an `ApiError` carrying the problem+json body.
 */

import { buildQuery, useApi } from '@/composables/useApi'
import type { OrderDetail, OrderFilter, OrderListItem } from '@/types/order'
import type { PagedResult } from '@/types/pagedresult'
import { ApiPath } from '@/lib/apiPaths'
import { HttpMethod } from '@/types/http'

/**
 * Endpoint functions for `/orders`.
 *
 * @returns One typed function per endpoint.
 */
export const useOrdersApi = () => {
  const { request } = useApi()

  /**
   * Lists orders, one page.
   *
   * @param filter - Page, page size and status.
   * @returns The page and the total count.
   */
  const listOrders = (filter: OrderFilter): Promise<PagedResult<OrderListItem>> =>
    request<PagedResult<OrderListItem>>(
      `${ApiPath.Orders}${buildQuery({ page: filter.page, pageSize: filter.pageSize, status: filter.status })}`,
    )

  /**
   * Reads one order with its lines and invoice.
   *
   * @param id - Order id.
   * @returns The order detail.
   */
  const getOrder = (id: number): Promise<OrderDetail> => request<OrderDetail>(`${ApiPath.Orders}/${id}`)

  /**
   * Accepts a pending order.
   *
   * @param id - Order id.
   * @returns Nothing.
   */
  const acceptOrder = (id: number): Promise<void> => request<void>(`${ApiPath.Orders}/${id}/accept`, { method: HttpMethod.Post })

  /**
   * Cancels a pending order.
   *
   * @param id - Order id.
   * @returns Nothing.
   */
  const cancelOrder = (id: number): Promise<void> => request<void>(`${ApiPath.Orders}/${id}/cancel`, { method: HttpMethod.Post })

  /**
   * Deletes an order.
   *
   * @param id - Order id.
   * @returns Nothing.
   */
  const deleteOrder = (id: number): Promise<void> => request<void>(`${ApiPath.Orders}/${id}`, { method: HttpMethod.Delete })

  return { listOrders, getOrder, acceptOrder, cancelOrder, deleteOrder }
}
