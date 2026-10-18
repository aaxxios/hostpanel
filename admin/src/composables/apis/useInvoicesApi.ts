/**
 * One function per endpoint on the API invoice surface (`/api/billing`). No state lives here,
 * no error handling: a refusal rejects with an `ApiError` and the invoices store turns it
 * into a sentence.
 *
 * Callers are the invoices store, never a `.vue` file.
 */

import { buildQuery, useApi } from '@/composables/useApi'
import type {
  CreatedId,
  CreateInvoicePayload,
  Invoice,
  InvoiceFilters,
  InvoiceItemUpdate,
  InvoiceOptionsPayload,
  InvoicePaymentPayload,
  InvoiceRefundPayload,
} from '@/types/invoice'
import type { PagedResult } from '@/types/pagedresult'

/**
 * Endpoint functions for `/api/billing` invoices.
 *
 * @returns One typed function per invoice endpoint the admin panel uses.
 */
export const useInvoicesApi = () => {
  const { request } = useApi()

  /**
   * Lists invoices across all clients.
   *
   * @param page - Page number (1-based).
   * @param pageSize - Items per page.
   * @param filters - Optional status and date filters.
   * @returns One page of invoices.
   */
  const list = (page: number, pageSize: number, filters: InvoiceFilters = {}): Promise<PagedResult<Invoice>> =>
    request<PagedResult<Invoice>>(`/billing${buildQuery({ page, pageSize, ...filters })}`)

  /**
   * Lists the invoices of one client.
   *
   * @param clientId - The client id.
   * @param page - Page number (1-based).
   * @param pageSize - Items per page.
   * @param filters - Optional status and date filters.
   * @returns One page of invoices.
   */
  const listByClient = (
    clientId: number | string,
    page: number,
    pageSize: number,
    filters: InvoiceFilters = {},
  ): Promise<PagedResult<Invoice>> =>
    request<PagedResult<Invoice>>(`/billing/client/${clientId}${buildQuery({ page, pageSize, ...filters })}`)

  /**
   * Loads one invoice with its lines and transactions.
   *
   * @param id - The invoice id.
   * @returns The invoice.
   */
  const get = (id: number): Promise<Invoice> => request<Invoice>(`/billing/${id}`)

  /**
   * Creates an invoice for a client.
   *
   * @param payload - Client, due date, lines and draft flag.
   * @returns The new invoice id.
   */
  const create = (payload: CreateInvoicePayload): Promise<CreatedId> =>
    request<CreatedId>('/billing', { method: 'POST', body: JSON.stringify(payload) })

  /**
   * Replaces the line items of an invoice.
   *
   * @param id - The invoice id.
   * @param items - The item changes to apply.
   * @returns Nothing.
   */
  const updateItems = (id: number, items: InvoiceItemUpdate[]): Promise<void> =>
    request<void>(`/billing/${id}/items`, { method: 'PUT', body: JSON.stringify({ items }) })

  /**
   * Updates dates, payment method and tax rate.
   *
   * @param id - The invoice id.
   * @param options - The new options.
   * @returns Nothing.
   */
  const updateOptions = (id: number, options: InvoiceOptionsPayload): Promise<void> =>
    request<void>(`/billing/${id}/options`, { method: 'PUT', body: JSON.stringify(options) })

  /**
   * Updates the admin notes.
   *
   * @param id - The invoice id.
   * @param notes - Notes text.
   * @returns Nothing.
   */
  const updateNotes = (id: number, notes: string): Promise<void> =>
    request<void>(`/billing/${id}/notes`, { method: 'PUT', body: JSON.stringify({ notes }) })

  /**
   * Publishes a draft invoice (Draft to Unpaid).
   *
   * @param id - The invoice id.
   * @returns Nothing.
   */
  const publish = (id: number): Promise<void> => request<void>(`/billing/${id}/publish`, { method: 'POST' })

  /**
   * Attempts to capture payment through the configured gateway.
   *
   * @param id - The invoice id.
   * @param currency - ISO 4217 currency to charge in.
   * @returns Nothing.
   */
  const pay = (id: number, currency: string): Promise<void> =>
    request<void>(`/billing/${id}/pay`, { method: 'POST', body: JSON.stringify({ currency }) })

  /**
   * Cancels an invoice.
   *
   * @param id - The invoice id.
   * @returns Nothing.
   */
  const cancel = (id: number): Promise<void> => request<void>(`/billing/${id}/cancel`, { method: 'POST' })

  /**
   * Records a payment against an invoice.
   *
   * @param id - The invoice id.
   * @param payment - Payment details.
   * @returns Nothing.
   */
  const addPayment = (id: number, payment: InvoicePaymentPayload): Promise<void> =>
    request<void>(`/billing/${id}/payment`, { method: 'POST', body: JSON.stringify(payment) })

  /**
   * Applies client credit to an invoice.
   *
   * @param id - The invoice id.
   * @param amount - Credit amount to apply.
   * @returns Nothing.
   */
  const applyCredit = (id: number, amount: number): Promise<void> =>
    request<void>(`/billing/${id}/credit`, { method: 'POST', body: JSON.stringify({ amount }) })

  /**
   * Removes applied credit from an invoice.
   *
   * @param id - The invoice id.
   * @param amount - Credit amount to remove.
   * @returns Nothing.
   */
  const removeCredit = (id: number, amount: number): Promise<void> =>
    request<void>(`/billing/${id}/credit/remove`, { method: 'POST', body: JSON.stringify({ amount }) })

  /**
   * Refunds a payment transaction.
   *
   * @param id - The invoice id.
   * @param payload - Refund details.
   * @returns Nothing.
   */
  const refund = (id: number, payload: InvoiceRefundPayload): Promise<void> =>
    request<void>(`/billing/${id}/refund`, { method: 'POST', body: JSON.stringify(payload) })

  /**
   * Duplicates an invoice.
   *
   * @param id - The invoice id to duplicate.
   * @returns The new invoice id.
   */
  const duplicate = (id: number): Promise<CreatedId> =>
    request<CreatedId>(`/billing/${id}/duplicate`, { method: 'POST' })

  /**
   * Deletes an invoice.
   *
   * @param id - The invoice id.
   * @returns Nothing.
   */
  const remove = (id: number): Promise<void> => request<void>(`/billing/${id}`, { method: 'DELETE' })

  /**
   * Applies one action to several invoices.
   *
   * @param invoiceIds - The invoice ids.
   * @param action - Action name, e.g. `MarkPaid`, `Duplicate`, `Delete`.
   * @returns Nothing.
   */
  const bulk = (invoiceIds: number[], action: string): Promise<void> =>
    request<void>('/billing/bulk', { method: 'POST', body: JSON.stringify({ invoiceIds, action }) })

  return {
    list,
    listByClient,
    get,
    create,
    updateItems,
    updateOptions,
    updateNotes,
    publish,
    pay,
    cancel,
    addPayment,
    applyCredit,
    removeCredit,
    refund,
    duplicate,
    remove,
    bulk,
  }
}
