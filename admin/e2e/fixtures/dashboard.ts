/**
 * Fixtures for the dashboard: the headline numbers `GET /api/admin/dashboard/stats` answers with.
 */
import type { DashboardStats } from '../../src/types/dashboardstats'

/** Distinct small numbers, so a tile showing the wrong field is caught. */
export const dashboardStats: DashboardStats = {
  totalRevenue: 125000,
  monthlyRevenue: 8400,
  activeServices: 37,
  overdueInvoices: 5,
  openTickets: 11,
  totalClients: 64,
  currency: 'USD',
}
