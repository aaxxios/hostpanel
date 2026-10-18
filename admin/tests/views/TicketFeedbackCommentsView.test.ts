/**
 * Tests of TicketFeedbackCommentsView: it mounts, shows its title and data, and shows the refusal sentence when the API
 * refuses.
 */
import { vi } from 'vitest'
import TicketFeedbackCommentsView from '@/views/TicketFeedbackCommentsView.vue'
import { json } from '../helpers/fetch'
import { runReportCase } from '../helpers/reportViews'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({
  name: 'TicketFeedbackCommentsView',
  view: TicketFeedbackCommentsView,
  routes: {
    'GET /reports/ticket-feedback-comments': () => json({
      rows: [{
        ticketId: 5, staffName: 'Ann', subject: 'Help', feedbackComment: 'Great service', rating: 9,
        feedbackLeftBy: 'Beta', feedbackAt: '2024-02-01T12:00:00Z',
      }],
    }),
  },
  expected: ['Ticket Feedback Comments', 'Great service', 'Beta', '01/02/2024'],
})
