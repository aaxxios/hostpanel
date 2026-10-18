/** Activity counters of one day. */
export interface ReportDailyPerformanceRow {
  /** The day. */
  date: string
  /** Orders completed. */
  completedOrders: number
  /** Invoices created. */
  newInvoices: number
  /** Invoices paid. */
  paidInvoices: number
  /** Tickets opened. */
  openedTickets: number
  /** Ticket replies written. */
  ticketReplies: number
  /** Cancellation requests received. */
  cancellationRequests: number
}
