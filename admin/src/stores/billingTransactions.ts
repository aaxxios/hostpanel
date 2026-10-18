/**
 * Pinia store for the billing transaction ledger (all clients).
 *
 * It is the only caller of `useTransactionsApi` for this screen; a refusal becomes the API
 * sentence in `error` (and the per-field messages in `fieldErrors`).
 */

import { defineStore } from 'pinia'
import { reactive, ref } from 'vue'
import { useTransactionsApi } from '@/composables/apis/useTransactionsApi'
import { runAction } from '@/lib/runAction'
import type { CreateTransactionPayload, Transaction, UpdateTransactionPayload } from '@/types/transaction'

/** Lists, creates, edits and deletes ledger transactions with their summary totals. */
export const useTransactionsStore = defineStore('billingTransactions', () => {
  const api = useTransactionsApi()

  /** Loaded transactions of the current page. */
  const transactions = ref<Transaction[]>([])

  /** Transaction open in the detail view. */
  const currentTransaction = ref<Transaction | null>(null)

  /** Total number of transactions across all pages. */
  const totalCount = ref(0)

  /** True while a request is in flight. */
  const loading = ref(false)

  /** Sentence for the last failed request, null when none. */
  const error = ref<string | null>(null)

  /** Per-field messages of the last refused request, keyed by camelCase field name. */
  const fieldErrors = ref<Record<string, string>>({})

  /** Current page (1-based). */
  const page = ref(1)

  /** Items per page. */
  const pageSize = ref(25)

  /** Financial summary totals of the whole ledger. */
  const summary = reactive({ totalIn: 0, totalOut: 0, totalFees: 0, balance: 0 })

  /**
   * Runs one request with the loading flag and the error state around it.
   *
   * @param task - The request to run.
   * @returns The outcome: the value on success, a failure whose sentence is in `error`.
   */
  const run = async <T>(task: () => Promise<T>) => {
    loading.value = true
    const result = await runAction(error, task, fieldErrors)
    loading.value = false
    return result
  }

  /**
   * Loads one transaction into `currentTransaction`.
   *
   * @param id - The transaction id.
   * @returns Promise that resolves when the transaction is loaded.
   */
  const fetchById = async (id: number): Promise<void> => {
    const result = await run(() => api.get(id))
    if (result.ok) {
      currentTransaction.value = result.value
    }
  }

  /**
   * Loads one page of the ledger with its totals.
   *
   * @param p - Page number (1-based).
   * @param ps - Items per page.
   * @returns Promise that resolves when data is loaded.
   */
  const fetchAll = async (p = 1, ps = 25): Promise<void> => {
    page.value = p
    pageSize.value = ps
    const result = await run(() => api.list(p, ps))
    if (result.ok) {
      transactions.value = result.value.transactions.items
      totalCount.value = result.value.transactions.totalCount
      summary.totalIn = result.value.totalIn
      summary.totalOut = result.value.totalOut
      summary.totalFees = result.value.totalFees
      summary.balance = result.value.balance
    }
  }

  /**
   * Creates a transaction, then reloads the current page.
   *
   * @param payload - Transaction fields.
   * @returns True when created; false when refused (see `error`).
   */
  const create = async (payload: CreateTransactionPayload): Promise<boolean> => {
    const result = await run(() => api.create(payload))
    if (result.ok) {
      await fetchAll(page.value, pageSize.value)
    }
    return result.ok
  }

  /**
   * Updates a transaction.
   *
   * @param id - The transaction id.
   * @param payload - Transaction fields.
   * @returns True when saved; false when refused (see `error`).
   */
  const update = async (id: number, payload: UpdateTransactionPayload): Promise<boolean> => {
    const result = await run(() => api.update(id, payload))
    return result.ok
  }

  /**
   * Deletes a transaction, then reloads the current page.
   *
   * @param id - The transaction id.
   * @returns True when deleted; false when refused (see `error`).
   */
  const remove = async (id: number): Promise<boolean> => {
    const result = await run(() => api.remove(id))
    if (result.ok) {
      await fetchAll(page.value, pageSize.value)
    }
    return result.ok
  }

  return {
    transactions,
    currentTransaction,
    totalCount,
    summary,
    loading,
    error,
    fieldErrors,
    page,
    pageSize,
    fetchAll,
    fetchById,
    create,
    update,
    remove,
  }
})
