/**
 * Fixtures for the transactions ledger: the entry the add flow creates, and the ledger before and
 * after it as `GET /api/transactions` returns it.
 */
import type { Transaction, TransactionsResult } from '../../src/types/transaction'

/** The entry created by the add flow. */
export const newTransaction: Transaction = {
  id: 701,
  clientId: 7,
  clientName: 'Ada Lovelace',
  date: '2026-10-14T00:00:00Z',
  description: 'Manual bank payment',
  transactionId: '',
  invoiceId: null,
  paymentMethod: '',
  amountIn: 100,
  amountOut: 0,
  fees: 0,
  addedToCredit: false,
}

/** A ledger with nothing in it. */
export const ledgerEmpty: TransactionsResult = {
  transactions: { items: [], totalCount: 0, page: 1, pageSize: 25 },
  totalIn: 0,
  totalOut: 0,
  totalFees: 0,
  balance: 0,
}

/** The ledger once the entry exists. */
export const ledgerAfterAdd: TransactionsResult = {
  transactions: { items: [newTransaction], totalCount: 1, page: 1, pageSize: 25 },
  totalIn: 100,
  totalOut: 0,
  totalFees: 0,
  balance: 100,
}
