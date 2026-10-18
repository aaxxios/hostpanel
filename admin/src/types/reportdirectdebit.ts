/** One unpaid direct-debit invoice. */
export interface ReportDirectDebitRow {
  /** Invoice id. */
  invoiceId: number
  /** Client name. */
  clientName: string
  /** Invoice date. */
  invoiceDate: string
  /** Due date. */
  dueDate: string
  /** Sub total. */
  subtotal: number
  /** Tax. */
  tax: number
  /** Credit applied. */
  credit: number
  /** Total. */
  total: number
  /** Bank name, when stored. */
  bankName: string | null
  /** Bank account type, when stored. */
  bankAccountType: string | null
  /** Bank code, when stored. */
  bankCode: string | null
  /** Bank account number, when stored. */
  bankAccountNumber: string | null
}

/** The direct debit report. */
export interface ReportDirectDebit {
  /** The unpaid invoices. */
  rows: ReportDirectDebitRow[]
}
