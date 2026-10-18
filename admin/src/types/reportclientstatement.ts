/** One entry of a client statement. */
export interface ReportStatementLine {
  /** Kind of entry (invoice, payment). */
  type: string
  /** Entry date. */
  date: string
  /** Description. */
  description: string
  /** Amount billed. */
  debit: number
  /** Amount paid. */
  credit: number
  /** Running balance after the entry. */
  balance: number
}

/** The statement of one client account. */
export interface ReportClientStatement {
  /** Client id. */
  clientId: number
  /** Client name. */
  clientName: string
  /** Balance before the first entry. */
  previousBalance: number
  /** The entries of the period. */
  lines: ReportStatementLine[]
  /** Balance after the last entry. */
  endingBalance: number
}
