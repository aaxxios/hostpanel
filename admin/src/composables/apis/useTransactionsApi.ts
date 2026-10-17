/**
 * One function per endpoint on the API transaction surface (`/api/transactions`), both the
 * billing ledger and the per-client view. No state lives here, no error handling.
 *
 * Callers are the two transaction stores, never a `.vue` file.
 */

import { buildQuery, useApi } from '@/composables/useApi'
import type {
  CreateClientTransactionPayload,
  CreateTransactionPayload,
  Transaction,
  TransactionsResult,
  UpdateTransactionPayload,
} from '@/types/transaction'

/**
 * Endpoint functions for transactions.
 *
 * @returns One typed function per transaction endpoint the admin panel uses.
 */
export const useTransactionsApi = () => {
  const { request } = useApi()

  /**
   * Lists transactions across all clients, with summary totals.
   *
   * @param page - Page number (1-based).
   * @param pageSize - Items per page.
   * @returns One page of transactions and the totals.
   */
  const list = (page: number, pageSize: number): Promise<TransactionsResult> =>
    request<TransactionsResult>(`/transactions${buildQuery({ page, pageSize })}`)

  /**
   * Lists the transactions of one client, with summary totals.
   *
   * @param clientId - The client id.
   * @param page - Page number (1-based).
   * @param pageSize - Items per page.
   * @returns One page of transactions and the totals.
   */
  const listByClient = (clientId: number | string, page: number, pageSize: number): Promise<TransactionsResult> =>
    request<TransactionsResult>(`/transactions/client/${clientId}${buildQuery({ page, pageSize })}`)

  /**
   * Loads one transaction.
   *
   * @param id - The transaction id.
   * @returns The transaction.
   */
  const get = (id: number): Promise<Transaction> => request<Transaction>(`/transactions/${id}`)

  /**
   * Creates a transaction.
   *
   * @param payload - Transaction fields.
   * @returns Nothing.
   */
  const create = (payload: CreateTransactionPayload | CreateClientTransactionPayload): Promise<void> =>
    request<void>('/transactions', { method: 'POST', body: JSON.stringify(payload) })

  /**
   * Updates a transaction.
   *
   * @param id - The transaction id.
   * @param payload - Transaction fields.
   * @returns Nothing.
   */
  const update = (id: number, payload: UpdateTransactionPayload): Promise<void> =>
    request<void>(`/transactions/${id}`, { method: 'PUT', body: JSON.stringify(payload) })

  /**
   * Deletes a transaction.
   *
   * @param id - The transaction id.
   * @returns Nothing.
   */
  const remove = (id: number): Promise<void> => request<void>(`/transactions/${id}`, { method: 'DELETE' })

  return { list, listByClient, get, create, update, remove }
}
