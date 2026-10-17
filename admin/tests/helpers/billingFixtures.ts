/**
 * Fixtures the billing screen tests share: an invoice with its lines and transactions, a quote
 * list row and a ledger transaction, shaped as the API sends them.
 */
import type { Invoice } from '@/types/invoice'
import type { QuoteListItem } from '@/types/quote'
import type { Transaction } from '@/types/transaction'

/**
 * Builds an invoice with one line, one payment and one refund.
 *
 * @param overrides - Fields to replace.
 * @returns The invoice.
 */
export const invoiceFixture = (overrides: Partial<Invoice> = {}): Invoice => ({
  id: 1,
  clientId: 7,
  status: 'Unpaid',
  dueDate: '2026-12-31T00:00:00Z',
  createdAt: '2026-10-14T09:00:00Z',
  invoiceDate: '2026-10-14T00:00:00Z',
  total: 100,
  subTotal: 100,
  tax: 0,
  taxRate: 0,
  credit: 5,
  clientName: 'Ada Lovelace',
  paymentMethod: 'Stripe',
  items: [{ id: 11, description: 'Managed hosting', unitPrice: 100, quantity: 1, amount: 100 }],
  transactions: [
    { id: 21, date: '2026-10-15T00:00:00Z', gateway: 'Stripe', transactionId: 'ch_1', amount: 40, fees: 1.5, type: 'Payment' },
    { id: 22, date: '2026-10-16T00:00:00Z', gateway: 'Stripe', transactionId: 'rf_1', amount: 10, fees: 0, type: 'Refund', notes: 'Goodwill' },
  ],
  ...overrides,
})

/**
 * Builds a quote list row.
 *
 * @param overrides - Fields to replace.
 * @returns The quote row.
 */
export const quoteRowFixture = (overrides: Partial<QuoteListItem> = {}): QuoteListItem => ({
  id: 3,
  clientId: 7,
  clientName: 'Ada Lovelace',
  subject: 'Hosting renewal',
  dateCreated: '2026-10-01T00:00:00Z',
  validUntil: '2099-12-31T00:00:00Z',
  total: 250,
  stage: 'Delivered',
  ...overrides,
})

/**
 * Builds a ledger transaction.
 *
 * @param overrides - Fields to replace.
 * @returns The transaction.
 */
export const transactionFixture = (overrides: Partial<Transaction> = {}): Transaction => ({
  id: 9,
  clientId: 7,
  clientName: 'Ada Lovelace',
  date: '2026-10-10T00:00:00Z',
  description: 'Hosting payment',
  transactionId: 'ch_9',
  invoiceId: 1,
  paymentMethod: 'Stripe',
  amountIn: 120,
  amountOut: 0,
  fees: 3.5,
  addedToCredit: false,
  ...overrides,
})
