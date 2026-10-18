/**
 * The four counters at the top of the client dashboard.
 *
 * @module types/dashboardstat
 */
export enum DashboardStat {
  /** Active hosting services. */
  Services = 'services',
  /** Registered domains. */
  Domains = 'domains',
  /** Invoices awaiting payment. */
  Invoices = 'invoices',
  /** Support tickets still open. */
  Tickets = 'tickets'
}
