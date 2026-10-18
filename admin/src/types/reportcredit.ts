/** One credit issued to a client. */
export interface ReportCreditRow {
  /** Credit id. */
  id: number
  /** Client id. */
  clientId: number
  /** Client name. */
  clientName: string
  /** Date issued. */
  date: string
  /** Description. */
  description: string
  /** Amount credited. */
  amount: number
  /** Staff member who issued it, when known. */
  adminUser: string | null
}

/** The credits reviewer report. */
export interface ReportCredits {
  /** Number of credits found. */
  totalCount: number
  /** Sum of the credits. */
  totalAmount: number
  /** The credits. */
  rows: ReportCreditRow[]
}
