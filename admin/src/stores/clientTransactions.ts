/**
 * Pinia store for the transactions of one client.
 *
 * Lists, creates and deletes transactions through `useTransactionsApi` and keeps the summary
 * totals beside the paged list. A refusal becomes the API sentence in `error` (and the
 * per-field messages in `fieldErrors`).
 */

import { defineStore } from 'pinia'
import { reactive, ref } from 'vue'
import { useTransactionsApi } from '@/composables/apis/useTransactionsApi'
import { runAction } from '@/lib/runAction'
import type { CreateClientTransactionPayload, Transaction } from '@/types/transaction'
import { LEDGER_PAGE_SIZE } from '@/lib/limits'
import { CurrencyCode } from '@/types/currency'

/** Lists, creates and deletes the transactions of a client. */
export const useClientTransactionsStore = defineStore('clientTransactions', () => {
  const api = useTransactionsApi()

  /** Loaded transactions for the current client. */
  const transactions = ref<Transaction[]>([])

  /** Total number of transactions across all pages. */
  const totalCount = ref(0)

  /** Financial summary totals for the client. */
  const summary = reactive({
    /** Sum of all AmountIn values. */
    totalIn: 0,
    /** Sum of all AmountOut values. */
    totalOut: 0,
    /** Sum of all Fees values. */
    totalFees: 0,
    /** Calculated balance: TotalIn - TotalOut - TotalFees. */
    balance: 0,
    /** ISO 4217 code of the base currency the totals are expressed in. */
    currency: CurrencyCode.Usd as string,
  })

  /** True while a request is in flight. */
  const loading = ref(false)

  /** Sentence for the last failed request, null when none. */
  const error = ref<string | null>(null)

  /** Per-field messages of the last refused request, keyed by camelCase field name. */
  const fieldErrors = ref<Record<string, string>>({})

  /**
   * Fetches transactions for a specific client with pagination.
   *
   * @param clientId - The client id.
   * @param page - Page number (1-based).
   * @param pageSize - Items per page.
   * @returns Promise that resolves when data is loaded.
   */
  const fetchByClient = async (clientId: number | string, page = 1, pageSize = LEDGER_PAGE_SIZE): Promise<void> => {
    loading.value = true
    const result = await runAction(error, () => api.listByClient(clientId, page, pageSize))
    loading.value = false
    if (result.ok) {
      transactions.value = result.value.transactions.items
      totalCount.value = result.value.transactions.totalCount
      summary.totalIn = result.value.totalIn
      summary.totalOut = result.value.totalOut
      summary.totalFees = result.value.totalFees
      summary.balance = result.value.balance
      summary.currency = result.value.currency
    }
  }

  /**
   * Creates a new transaction.
   *
   * @param payload - Transaction data to submit.
   * @returns True when created; false when refused (see `error`).
   */
  const create = async (payload: CreateClientTransactionPayload): Promise<boolean> => {
    loading.value = true
    const result = await runAction(error, () => api.create(payload), fieldErrors)
    loading.value = false
    return result.ok
  }

  /**
   * Deletes a transaction by id.
   *
   * @param id - The transaction id to delete.
   * @returns True when deleted; false when refused (see `error`).
   */
  const remove = async (id: number): Promise<boolean> => {
    loading.value = true
    const result = await runAction(error, () => api.remove(id))
    loading.value = false
    return result.ok
  }

  return { transactions, totalCount, summary, loading, error, fieldErrors, fetchByClient, create, remove }
})
