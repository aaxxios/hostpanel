/** Projected income of one month, split by billing cycle. */
export interface ReportForecastRow {
  /** Month caption. */
  month: string
  /** Income from monthly services. */
  monthly: number
  /** Income from quarterly services. */
  quarterly: number
  /** Income from semi-annual services. */
  semiAnnual: number
  /** Income from annual services. */
  annual: number
  /** Sum of the above. */
  total: number
}
