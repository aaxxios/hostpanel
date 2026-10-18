/**
 * One function per endpoint on the API billable-item surface: the admin-wide list under
 * `/api/billing/billable-items` and the client-scoped one under
 * `/api/clients/{id}/billable-items`. No state lives here, no error handling.
 *
 * Callers are the billable-item stores, never a `.vue` file.
 */

import { buildQuery, useApi } from '@/composables/useApi'
import type { BillableItem, BillableItemsResult, CreateBillableItemPayload } from '@/types/billableitem'
import type { PagedResult } from '@/types/pagedresult'
import type { ServiceListItem } from '@/types/service'

/**
 * Endpoint functions for billable items.
 *
 * @returns One typed function per billable-item endpoint the admin panel uses.
 */
export const useBillableItemsApi = () => {
  const { request } = useApi()

  /**
   * Lists billable items across all clients.
   *
   * @param page - Page number (1-based).
   * @param pageSize - Items per page.
   * @param type - Optional type filter (`OneTime` or `Recurring`).
   * @returns One page of items.
   */
  const list = (page: number, pageSize: number, type?: string): Promise<PagedResult<BillableItem>> =>
    request<PagedResult<BillableItem>>(`/billing/billable-items${buildQuery({ page, pageSize, type })}`)

  /**
   * Creates a billable item.
   *
   * @param payload - Item fields.
   * @returns Nothing.
   */
  const create = (payload: CreateBillableItemPayload): Promise<void> =>
    request<void>('/billing/billable-items', { method: 'POST', body: JSON.stringify(payload) })

  /**
   * Deletes a billable item.
   *
   * @param id - The item id.
   * @returns Nothing.
   */
  const remove = (id: number): Promise<void> => request<void>(`/billing/billable-items/${id}`, { method: 'DELETE' })

  /**
   * Loads a client uninvoiced items and one page of invoiced items.
   *
   * @param clientId - The client id.
   * @param invoicedPage - Invoiced page, 1-based.
   * @param invoicedPageSize - Invoiced items per page.
   * @returns The client items.
   */
  const listByClient = (
    clientId: string,
    invoicedPage: number,
    invoicedPageSize: number,
  ): Promise<BillableItemsResult> =>
    request<BillableItemsResult>(
      `/clients/${clientId}/billable-items${buildQuery({ invoicedPage, invoicedPageSize })}`,
    )

  /**
   * Invoices the given uninvoiced items of a client.
   *
   * @param clientId - The client id.
   * @param billableItemIds - The item ids to invoice.
   * @returns Nothing.
   */
  const invoiceSelected = (clientId: string, billableItemIds: number[]): Promise<void> =>
    request<void>(`/clients/${clientId}/billable-items/invoice-selected`, {
      method: 'POST',
      body: JSON.stringify({ billableItemIds }),
    })

  /**
   * Deletes one billable item of a client.
   *
   * @param clientId - The client id.
   * @param id - The item id.
   * @returns Nothing.
   */
  const removeForClient = (clientId: string, id: number): Promise<void> =>
    request<void>(`/clients/${clientId}/billable-items/${id}`, { method: 'DELETE' })

  /**
   * Lists a client services for the item dropdowns.
   *
   * @param clientId - The client id.
   * @returns One page of services (up to 100).
   */
  const listServices = (clientId: string): Promise<PagedResult<ServiceListItem>> =>
    request<PagedResult<ServiceListItem>>(`/services${buildQuery({ clientId, pageSize: 100 })}`)

  /**
   * Adds one billable item to a client.
   *
   * @param clientId - The client id.
   * @param payload - The item fields.
   * @returns Nothing.
   */
  const addForClient = (clientId: string, payload: Record<string, unknown>): Promise<void> =>
    request<void>(`/clients/${clientId}/billable-items`, { method: 'POST', body: JSON.stringify(payload) })

  /**
   * Adds time-billing entries to a client.
   *
   * @param clientId - The client id.
   * @param entries - The entries to bill.
   * @returns Nothing.
   */
  const addTimeBilling = (clientId: string, entries: Record<string, unknown>[]): Promise<void> =>
    request<void>(`/clients/${clientId}/billable-items/time-billing`, {
      method: 'POST',
      body: JSON.stringify({ entries }),
    })

  return {
    list,
    create,
    remove,
    listByClient,
    invoiceSelected,
    removeForClient,
    listServices,
    addForClient,
    addTimeBilling,
  }
}
