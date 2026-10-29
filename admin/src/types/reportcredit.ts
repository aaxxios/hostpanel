import type { PaginatedList } from '@/types/paginatedlist'

/** One credit issued to a client. */
export interface ReportCreditRow {
  /** Ledger row id. */
  id: number
  /** Client id. */
  clientId: number
  /** Client name. */
  clientName: string
  /** Day issued, `yyyy-MM-dd`. */
  date: string
  /** Description. */
  description: string
  /** ISO 4217 code the amount is in. */
  currency: string
  /** Amount credited. */
  amount: number
}

/** The count and sum of the matching credits in one currency. */
export interface ReportCreditTotal {
  /** ISO 4217 code. */
  currency: string
  /** Number of credits. */
  totalCount: number
  /** Sum of the credits. */
  totalAmount: number
}

/** The credits reviewer report. */
export interface ReportCredits {
  /** Totals per currency over every matching credit. */
  totals: ReportCreditTotal[]
  /** One page of the credits, newest first. */
  rows: PaginatedList<ReportCreditRow>
}
