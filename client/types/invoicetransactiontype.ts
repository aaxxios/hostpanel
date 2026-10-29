/**
 * Kinds of money movement recorded against an invoice, as the API names them on
 * `ClientInvoiceTransaction.type`.
 *
 * @module types/invoicetransactiontype
 */

/** The kind of an invoice transaction, member names verbatim as the API sends them. */
export enum InvoiceTransactionType {
  /** A payment made towards the invoice. */
  Payment = 'Payment',
  /** Money returned to the client; recorded as a positive amount. */
  Refund = 'Refund',
  /** A credit applied to the invoice; already netted out of its total. */
  Credit = 'Credit'
}
