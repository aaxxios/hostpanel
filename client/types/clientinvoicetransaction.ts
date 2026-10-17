/**
 * One financial transaction recorded against an invoice, as `InvoiceTransactionDto` sends it.
 *
 * @module types/clientinvoicetransaction
 */

/** One financial transaction recorded against an invoice, as `InvoiceTransactionDto` sends it. */
export interface ClientInvoiceTransaction {
  /** Transaction primary key. */
  id: number
  /** UTC timestamp of the transaction, ISO 8601. */
  date: string
  /** Payment gateway name. */
  gateway: string
  /** External transaction reference. */
  transactionId: string
  /** Transaction amount. */
  amount: number
  /** Fees charged by the gateway. */
  fees: number
  /** Transaction type, as `InvoiceTransactionType` serialises. */
  type: string
  /** Optional notes; null when not provided. */
  notes: string | null
}
