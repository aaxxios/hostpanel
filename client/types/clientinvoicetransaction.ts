/**
 * One financial transaction recorded against an invoice, as `MyInvoiceTransactionDto` sends it.
 * The gateway's own reference, its fee and staff notes are not part of the client's view.
 *
 * @module types/clientinvoicetransaction
 */

import type { InvoiceTransactionType } from '~/types/invoicetransactiontype'

/** One financial transaction recorded against an invoice, as `MyInvoiceTransactionDto` sends it. */
export interface ClientInvoiceTransaction {
  /** Transaction primary key. */
  id: number
  /** UTC timestamp of the transaction, ISO 8601. */
  date: string
  /** Transaction amount, always positive; {@link ClientInvoiceTransaction.type} says which way the money moved. */
  amount: number
  /** Which way the money moved. */
  type: InvoiceTransactionType
}
