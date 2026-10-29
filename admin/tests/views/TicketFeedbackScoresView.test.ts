/**
 * Tests of TicketFeedbackScoresView: it mounts, shows its title and data, and shows the refusal sentence with Retry when the API
 * refuses.
 */
import { vi } from 'vitest'
import TicketFeedbackScoresView from '@/views/TicketFeedbackScoresView.vue'
import { json } from '../helpers/fetch'
import { runReportCase } from '../helpers/reportViews'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({
  name: 'TicketFeedbackScoresView',
  view: TicketFeedbackScoresView,
  routes: {
    'GET /reports/ticket-feedback-scores': () => json({
      rows: [{
        staffName: 'Ann', r1: 0, r2: 0, r3: 0, r4: 0, r5: 0, r6: 0, r7: 0, r8: 0, r9: 0, r10: 2, totalRatings: 2, averageRating: 10,
      }],
    }),
  },
  expected: ['Ticket Feedback Scores', 'Ann', '10.00'],
  retryable: true,
})
