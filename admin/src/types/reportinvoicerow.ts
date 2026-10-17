/** One row of the invoices report. */
export interface ReportInvoiceRow {
  /** Invoice id. */
  id: number
  /** Client id. */
  clientId: number
  /** Client name. */
  clientName: string
  /** Invoice number, when numbering is on. */
  invoiceNumber: string | null
  /** Creation date. */
  createdDate: string
  /** Due date. */
  dueDate: string
  /** Date paid, when paid. */
  datePaid: string | null
  /** Date refunded, when refunded. */
  dateRefunded: string | null
  /** Date cancelled, when cancelled. */
  dateCancelled: string | null
  /** Sub total. */
  subTotal: number
  /** Credit applied. */
  credit: number
  /** First tax amount. */
  tax: number
  /** Second tax amount. */
  tax2: number
  /** First tax rate in percent. */
  taxRate: number
  /** Second tax rate in percent. */
  taxRate2: number
  /** Total. */
  total: number
  /** Invoice status. */
  status: string
  /** Payment method, when set. */
  paymentMethod: string | null
  /** Staff notes. */
  notes: string | null
}
