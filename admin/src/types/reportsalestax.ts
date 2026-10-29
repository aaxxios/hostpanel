import type { PaginatedList } from '@/types/paginatedlist'

/** One invoice of the sales tax report. */
export interface ReportSalesTaxRow {
  /** Invoice id. */
  id: number
  /** Client name. */
  clientName: string
  /** Day raised, `yyyy-MM-dd`. */
  invoiceDate: string
  /** Day paid, `yyyy-MM-dd`. */
  datePaid: string
  /** ISO 4217 code every amount on the row is in. */
  currency: string
  /** Sub total. */
  subTotal: number
  /** Tax. */
  tax: number
  /** Credit applied. */
  credit: number
  /** Total. */
  total: number
}

/** The sales tax totals of one invoice currency. */
export interface ReportSalesTaxTotal {
  /** ISO 4217 code. */
  currency: string
  /** Number of invoices paid in the period. */
  totalInvoices: number
  /** Sum of their totals. */
  totalInvoiced: number
  /** Sum of their tax. */
  taxLiability: number
}

/** The sales tax report. */
export interface ReportSalesTax {
  /** Totals per currency over every matching invoice. */
  totals: ReportSalesTaxTotal[]
  /** One page of the invoices, in the order they were paid. */
  rows: PaginatedList<ReportSalesTaxRow>
}
