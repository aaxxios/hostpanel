/**
 * Helpers the add and edit transaction screens share: the empty form, and the mapping from the
 * form's comma-separated invoice ids to the single invoice the API accepts.
 */

import { toIsoDay } from '@/lib/format'
import type { Transaction, TransactionFormValues } from '@/types/transaction'

/**
 * Builds an empty transaction form dated today.
 *
 * @returns The blank form values.
 */
export const blankTransactionForm = (): TransactionFormValues => ({
  date: toIsoDay(new Date()),
  clientId: '',
  transactionId: '',
  paymentMethod: '',
  description: '',
  amountIn: 0,
  amountOut: 0,
  fees: 0,
  invoiceIds: '',
  addToCredit: false,
})

/**
 * Fills the form from a stored transaction.
 *
 * @param transaction - The transaction being edited.
 * @returns The form values describing it.
 */
export const formFromTransaction = (transaction: Transaction): TransactionFormValues => {
  const parsed = new Date(transaction.date)
  return {
    date: Number.isNaN(parsed.getTime()) ? toIsoDay(new Date()) : toIsoDay(parsed),
    clientId: String(transaction.clientId),
    transactionId: transaction.transactionId || '',
    paymentMethod: transaction.paymentMethod || '',
    description: transaction.description,
    amountIn: transaction.amountIn,
    amountOut: transaction.amountOut,
    fees: transaction.fees,
    invoiceIds: transaction.invoiceId ? String(transaction.invoiceId) : '',
    addToCredit: transaction.addedToCredit,
  }
}

/**
 * Reads the invoice a transaction is linked to from the form's comma-separated ids.
 *
 * The form takes a list but the API accepts a single optional invoice, so the first entry wins.
 *
 * @param invoiceIds - The ids as typed, e.g. `"12, 15"`.
 * @returns The first id, or null when none was typed.
 */
export const firstInvoiceId = (invoiceIds: string): number | null => {
  const first = invoiceIds
    .split(',')
    .map((part) => part.trim())
    .filter(Boolean)[0]
  return first ? Number.parseInt(first, 10) : null
}
