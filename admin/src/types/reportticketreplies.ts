/** Ticket replies written by one staff member in a month. */
export interface ReportTicketReplyRow {
  /** Staff member. */
  adminName: string
  /** Replies per day of the month, day 1 first. */
  dailyCounts: number[]
  /** Replies in the month. */
  total: number
}

/** The support ticket replies report. */
export interface ReportTicketReplies {
  /** Month number, 1 to 12. */
  month: number
  /** Year. */
  year: number
  /** Days in the month. */
  daysInMonth: number
  /** One row per staff member. */
  rows: ReportTicketReplyRow[]
}
