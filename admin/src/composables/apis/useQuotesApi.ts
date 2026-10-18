/**
 * One function per endpoint on the API quote surface (`/api/billing/quotes`, `/api/quotes`).
 * No state lives here, no error handling: a refusal rejects with an `ApiError` and the quote
 * store turns it into a sentence.
 *
 * Callers are the quote store and the client summary store, never a `.vue` file.
 */

import { buildQuery, useApi } from '@/composables/useApi'
import type { PagedResult } from '@/types/pagedresult'
import type { CreateQuotePayload, Quote, QuoteListItem, UpdateQuotePayload } from '@/types/quote'

/**
 * Endpoint functions for quotes.
 *
 * @returns One typed function per quote endpoint the admin panel uses.
 */
export const useQuotesApi = () => {
  const { request } = useApi()

  /**
   * Lists quotes across all clients.
   *
   * @param page - Page number (1-based).
   * @param pageSize - Items per page.
   * @returns One page of quotes.
   */
  const list = (page: number, pageSize: number): Promise<PagedResult<QuoteListItem>> =>
    request<PagedResult<QuoteListItem>>(`/billing/quotes${buildQuery({ page, pageSize })}`)

  /**
   * Lists the quotes of one client.
   *
   * @param clientId - The client id.
   * @param page - Page number (1-based).
   * @param pageSize - Items per page.
   * @returns One page of quotes.
   */
  const listByClient = (clientId: number | string, page: number, pageSize: number): Promise<PagedResult<QuoteListItem>> =>
    request<PagedResult<QuoteListItem>>(`/billing/quotes/client/${clientId}${buildQuery({ page, pageSize })}`)

  /**
   * Loads one quote with its lines.
   *
   * @param id - The quote id.
   * @returns The quote.
   */
  const get = (id: number): Promise<Quote> => request<Quote>(`/quotes/${id}`)

  /**
   * Creates a quote.
   *
   * @param payload - Quote fields and lines.
   * @returns The new quote id.
   */
  const create = (payload: CreateQuotePayload): Promise<number> =>
    request<number>('/billing/quotes', { method: 'POST', body: JSON.stringify(payload) })

  /**
   * Updates a quote.
   *
   * @param id - The quote id.
   * @param payload - Quote fields and lines.
   * @returns Nothing.
   */
  const update = (id: number, payload: UpdateQuotePayload): Promise<void> =>
    request<void>(`/quotes/${id}`, { method: 'PUT', body: JSON.stringify(payload) })

  /**
   * Duplicates a quote.
   *
   * @param id - The quote id.
   * @returns The new quote id.
   */
  const duplicate = (id: number): Promise<number> => request<number>(`/quotes/${id}/duplicate`, { method: 'POST' })

  /**
   * Converts a quote to an invoice.
   *
   * @param id - The quote id.
   * @returns The new invoice id.
   */
  const convertToInvoice = (id: number): Promise<number> =>
    request<number>(`/quotes/${id}/convert-to-invoice`, { method: 'POST' })

  /**
   * Deletes a quote.
   *
   * @param id - The quote id.
   * @returns Nothing.
   */
  const remove = (id: number): Promise<void> => request<void>(`/quotes/${id}`, { method: 'DELETE' })

  return { list, listByClient, get, create, update, duplicate, convertToInvoice, remove }
}
