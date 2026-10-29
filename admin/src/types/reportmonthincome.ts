/** Income of one calendar month (annual income report). */
export interface ReportMonthIncome {
  /** Month caption. */
  month: string
  /** Income received in the month, payments less refunds, in `currency`. */
  amount: number
  /** ISO 4217 code of the base currency the amount is converted to. */
  currency: string
}
