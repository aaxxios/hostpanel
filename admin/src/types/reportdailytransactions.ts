/** Money movement of one day. */
export interface ReportDailyTransactionRow {
  /** The day. */
  date: string
  /** Money received. */
  amountIn: number
  /** Fees charged. */
  fees: number
  /** Money paid out. */
  amountOut: number
  /** Balance after the day. */
  balance: number
}

/** The monthly transactions report. */
export interface ReportDailyTransactions {
  /** Month number, 1 to 12. */
  month: number
  /** Year. */
  year: number
  /** One row per day. */
  rows: ReportDailyTransactionRow[]
  /** Money received in the month. */
  totalAmountIn: number
  /** Fees charged in the month. */
  totalFees: number
  /** Money paid out in the month. */
  totalAmountOut: number
}
