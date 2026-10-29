/** Aging bucket of an overdue invoice, exactly as the API sends it. */
export enum ReportAgingBucket {
  Days0To30 = 'Days0To30',
  Days30To60 = 'Days30To60',
  Days60To90 = 'Days60To90',
  Days90To120 = 'Days90To120',
  Over120 = 'Over120',
}

/** One overdue period of the aging invoices report. */
export interface ReportAgingPeriod {
  /** The aging bucket the row totals. */
  period: ReportAgingBucket
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
