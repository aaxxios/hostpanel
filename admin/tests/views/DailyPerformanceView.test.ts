/**
 * Tests of DailyPerformanceView: it mounts, shows its title and data, and shows the refusal sentence with Retry when the API
 * refuses.
 */
import { vi } from 'vitest'
import DailyPerformanceView from '@/views/DailyPerformanceView.vue'
import { json } from '../helpers/fetch'
import { runReportCase } from '../helpers/reportViews'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({
  name: 'DailyPerformanceView',
  view: DailyPerformanceView,
  routes: {
    'GET /reports/daily-performance': () => json([{
      date: '2024-01-01', completedOrders: 1, newInvoices: 2, paidInvoices: 3, openedTickets: 4, ticketReplies: 5, cancellationRequests: 6,
    }]),
  },
  expected: ['Daily Performance', '2024-01-01', 'Cancellation Requests'],
  retryable: true,
})
