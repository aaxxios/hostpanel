/** One client of the top clients by income report. */
export interface ReportTopClient {
  /** Client id. */
  clientId: number
  /** Client name. */
  clientName: string
  /** Money taken on the client's invoices. */
  totalAmountIn: number
  /** Gateway fees recorded on those payments and refunds. */
  totalFees: number
  /** Money refunded on the client's invoices. */
  totalAmountOut: number
  /** Net income: money taken less refunds and fees. */
  balance: number
  /** ISO 4217 code of the base currency the amounts are converted to. */
  currency: string
}
