/** One client of the top clients by income report. */
export interface ReportTopClient {
  /** Client id. */
  clientId: number
  /** Client name. */
  clientName: string
  /** Money received from the client. */
  totalAmountIn: number
  /** Fees charged on the client payments. */
  totalFees: number
  /** Money paid out to the client. */
  totalAmountOut: number
  /** Net balance. */
  balance: number
}
