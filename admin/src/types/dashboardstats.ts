/** Stats shown on the admin dashboard. */
export interface DashboardStats {
  /** Income across all time, payments less refunds, in `currency`. */
  totalRevenue: number
  /** Income of the last 30 days, in `currency`. */
  monthlyRevenue: number
  /** Number of active services. */
  activeServices: number
  /** Number of overdue invoices. */
  overdueInvoices: number
  /** Number of open support tickets. */
  openTickets: number
  /** Total registered clients. */
  totalClients: number
  /** ISO 4217 code of the base currency the revenue figures are converted to. */
  currency: string
}

/** The fields of `DashboardStats` that hold a number a tile shows. */
export type DashboardFigure = Exclude<keyof DashboardStats, 'currency'>
