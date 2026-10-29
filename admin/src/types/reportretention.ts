import type { BillingCycle } from '@/types/service'

/** What a retention group holds, exactly as the API sends it. */
export enum ReportRetentionGroupKind {
  /** The services of one product group. */
  ProductGroup = 'ProductGroup',
  /** The registered domains, one row per top-level domain. */
  Domains = 'Domains',
}

/** Retention figures of one product and billing cycle. */
export interface ReportRetentionRow {
  /** Product name, or the top-level domain. */
  productName: string
  /** Billing cycle of a service; null for a domain. */
  billingCycle: BillingCycle | null
  /** Registration term of a domain in years; null for a service. */
  termYears: number | null
  /** Number of services counted. */
  productCount: number
  /** Average days the services were active. */
  avgDaysActive: number
  /** Whole years in the average. */
  avgYears: number
  /** Whole months left after the years. */
  avgMonths: number
}

/** Retention rows of one group. */
export interface ReportRetentionGroup {
  /** What the group holds. */
  kind: ReportRetentionGroupKind
  /** Product group name; null for the domains group, which the screen labels itself. */
  groupName: string | null
  /** Rows of the group. */
  rows: ReportRetentionRow[]
}

/** The customer retention report. */
export interface ReportRetention {
  /** Groups. */
  groups: ReportRetentionGroup[]
}
