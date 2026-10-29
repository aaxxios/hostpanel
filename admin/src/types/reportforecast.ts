/** Income of one month with its trailing windows; every amount is in `currency`. */
export interface ReportForecastRow {
  /** Month caption. */
  month: string
  /** Income booked in this month alone. */
  monthly: number
  /** Income over this month and the two before it. */
  quarterly: number
  /** Income over this month and the five before it. */
  semiAnnual: number
  /** Income over the whole year. */
  annual: number
  /** Income from January up to and including this month. */
  total: number
  /** ISO 4217 code of the base currency the amounts are converted to. */
  currency: string
}
