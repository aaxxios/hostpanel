/** Retention figures of one product and billing cycle. */
export interface ReportRetentionRow {
  /** Product name. */
  productName: string
  /** Billing cycle. */
  billingCycle: string
  /** Number of services counted. */
  productCount: number
  /** Average days the services were active. */
  avgDaysActive: number
  /** The same average as years and months. */
  avgYearsMonthsActive: string
}

/** Retention rows of one product group. */
export interface ReportRetentionGroup {
  /** Group name. */
  groupName: string
  /** Rows of the group. */
  rows: ReportRetentionRow[]
}

/** The customer retention report. */
export interface ReportRetention {
  /** Product groups. */
  groups: ReportRetentionGroup[]
}
