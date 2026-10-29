/**
 * The row shape of the invoices report.
 */

import type { InvoiceAmounts, InvoiceStatus } from '@/types/invoice'

/** The dates of an invoice row, each `yyyy-MM-dd`. */
export interface ReportInvoiceDates {
  /** Creation date. */
  created: string
  /** Due date. */
  due: string
  /** Date paid, when paid. */
  paid: string | null
  /** Day of the latest refund; null when nothing was refunded. */
  refunded: string | null
}

/** One row of the invoices report. */
export interface ReportInvoiceRow {
  /** Invoice id. */
  id: number
  /** Client id. */
  clientId: number
  /** Client name. */
  clientName: string
  /** Created, due, paid and refunded dates. */
  dates: ReportInvoiceDates
  /** ISO 4217 code the invoice bills in; every amount on the row is in it. */
  currency: string
  /** Sub total, credit, tax, tax rate and total. */
  amounts: InvoiceAmounts
  /** Invoice status. */
  status: InvoiceStatus
  /** Payment method, when set. */
  paymentMethod: string | null
  /** Staff notes. */
  notes: string | null
}
