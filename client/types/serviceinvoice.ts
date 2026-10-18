/** One invoice charged to a service, as `GET /me/services/:id/invoices` returns it. */
export interface ServiceInvoice {
  /** Invoice primary key. */
  id: number
  /** Issue date (UTC, ISO 8601). */
  invoiceDate: string
  /** Payment due date (UTC, ISO 8601). */
  dueDate: string
  /** Grand total after tax and credit. */
  total: number
  /** ISO 4217 code this invoice was raised and is payable in, fixed at creation. */
  currency: string
  /** Lifecycle status name, e.g. `Unpaid`, `Paid`, `Overdue`. */
  status: string
}
