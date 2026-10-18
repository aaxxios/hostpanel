/** One invoice of the sales tax report. */
export interface ReportSalesTaxRow {
  /** Invoice id. */
  id: number
  /** Client name. */
  clientName: string
  /** Invoice date. */
  invoiceDate: string
  /** Date paid, when paid. */
  datePaid: string | null
  /** Sub total. */
  subTotal: number
  /** Tax. */
  tax: number
  /** Credit applied. */
  credit: number
  /** Total. */
  total: number
}

/** The sales tax report. */
export interface ReportSalesTax {
  /** Number of invoices found. */
  totalInvoices: number
  /** Sum invoiced. */
  totalInvoiced: number
  /** Liability at the first tax level. */
  taxLevel1Liability: number
  /** Liability at the second tax level. */
  taxLevel2Liability: number
  /** The invoices. */
  rows: ReportSalesTaxRow[]
}
