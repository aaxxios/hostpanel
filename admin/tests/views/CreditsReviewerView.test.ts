/**
 * Tests of CreditsReviewerView: it mounts, shows its title and data, and shows the refusal sentence when the API
 * refuses.
 */
import { vi } from 'vitest'
import CreditsReviewerView from '@/views/CreditsReviewerView.vue'
import { json } from '../helpers/fetch'
import { runReportCase } from '../helpers/reportViews'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({
  name: 'CreditsReviewerView',
  view: CreditsReviewerView,
  routes: {
    'GET /reports/credits-reviewer': () => json({
      totalCount: 1,
      totalAmount: 5,
      rows: [{ id: 1, clientId: 2, clientName: 'Beta', date: '2024-01-01', description: 'Gift', amount: 5, adminUser: null }],
    }),
  },
  expected: ['Credits Reviewer', 'Beta', 'Gift', '$5.00'],
})
