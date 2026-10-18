/**
 * Fixtures for the reports: the ranked clients `GET /api/reports/top-clients-by-income` returns.
 */
import type { ReportTopClient } from '../../src/types/reporttopclient'

/** Two clients ranked by income. */
export const topClients: ReportTopClient[] = [
  { clientId: 7, clientName: 'Ada Lovelace', totalAmountIn: 1200, totalFees: 30, totalAmountOut: 100, balance: 1070 },
  { clientId: 8, clientName: 'Grace Hopper', totalAmountIn: 800, totalFees: 20, totalAmountOut: 0, balance: 780 },
]
