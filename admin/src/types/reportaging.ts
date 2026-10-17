/** One overdue period of the aging invoices report. */
export interface ReportAgingPeriod {
  /** Period caption, for example the first thirty days. */
  period: string
  /** Overdue amount per currency code. */
  amountsByCurrency: Record<string, number>
}

/** The aging invoices report. */
export interface ReportAging {
  /** One row per overdue period. */
  periods: ReportAgingPeriod[]
  /** Total per currency code. */
  totals: Record<string, number>
  /** Currency codes that appear in the report, in column order. */
  currencies: string[]
}
