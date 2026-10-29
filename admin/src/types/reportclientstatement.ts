/** What a client statement entry records, exactly as the API sends it. */
export enum ReportStatementLineType {
  /** An invoice charged to the client. */
  Invoice = 'Invoice',
  /** A transaction recorded for the client. */
  Transaction = 'Transaction',
  /** The payment of an invoice that arrived already paid; no transaction was recorded for it here. */
  PaidHistory = 'PaidHistory',
}

/** One entry of a client statement. */
export interface ReportStatementLine {
  /** Kind of entry. */
  type: ReportStatementLineType
  /** Entry day, `yyyy-MM-dd`. */
  date: string
  /** Invoice the entry records; null for a transaction. */
  invoiceId: number | null
  /** Transaction description; null for an invoice or a history payment, whose text is built from the invoice id. */
  description: string | null
  /** ISO 4217 currency code every amount of the entry is in. */
  currency: string
  /** Amount billed or paid out. */
  debit: number
  /** Amount paid in. */
  credit: number
  /** Running balance of the entry's currency after the entry; positive means the client owes. */
  balance: number
}

/** The opening and closing balance of a client statement in one currency. */
export interface ReportStatementBalance {
  /** ISO 4217 currency code the balances are in. */
  currency: string
  /** Balance before the first entry; positive means the client owes. */
  previousBalance: number
  /** Balance after the last entry; positive means the client owes. */
  endingBalance: number
}

/** The statement of one client account; amounts of different currencies are never added. */
export interface ReportClientStatement {
  /** Client id. */
  clientId: number
  /** Client name. */
  clientName: string
  /** One opening and closing balance per currency the client has activity in. */
  balances: ReportStatementBalance[]
  /** The entries of the period. */
  lines: ReportStatementLine[]
}
