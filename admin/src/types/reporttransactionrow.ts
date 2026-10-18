/** One row of the transactions report. */
export interface ReportTransactionRow {
  /** Transaction id. */
  id: number
  /** Client id. */
  clientId: number
  /** Client name. */
  clientName: string
  /** Currency code. */
  currency: string
  /** Payment method. */
  paymentMethod: string
  /** Transaction date. */
  date: string
  /** Description. */
  description: string
  /** Invoice the transaction paid, when any. */
  invoiceId: number | null
  /** Gateway transaction id. */
  transactionId: string
  /** Money received. */
  amountIn: number
  /** Fees charged. */
  fees: number
  /** Money paid out. */
  amountOut: number
}
