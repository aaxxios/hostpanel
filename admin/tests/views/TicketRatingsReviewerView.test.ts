/**
 * Tests of TicketRatingsReviewerView: it mounts, shows its title and data, and shows the refusal sentence with Retry when the API
 * refuses.
 */
import { vi } from 'vitest'
import TicketRatingsReviewerView from '@/views/TicketRatingsReviewerView.vue'
import { json } from '../helpers/fetch'
import { runReportCase } from '../helpers/reportViews'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({
  name: 'TicketRatingsReviewerView',
  view: TicketRatingsReviewerView,
  routes: {
    'GET /reports/ticket-ratings-reviewer': () => json({
      rows: [{ ticketId: 5, date: '2024-02-01T12:00:00Z', message: 'Thanks a lot', adminName: 'Ann', rating: 9 }],
    }),
  },
  expected: ['Support Ticket Ratings Reviewer', 'Thanks a lot', '#5'],
  retryable: true,
})
