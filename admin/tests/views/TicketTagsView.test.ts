/**
 * Tests of TicketTagsView: it mounts, shows its title and data, and shows the refusal sentence with Retry when the API
 * refuses.
 */
import { vi } from 'vitest'
import TicketTagsView from '@/views/TicketTagsView.vue'
import { json } from '../helpers/fetch'
import { runReportCase } from '../helpers/reportViews'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({
  name: 'TicketTagsView',
  view: TicketTagsView,
  routes: { 'GET /reports/ticket-tags': () => json({ rows: [{ tag: 'billing', count: 4 }] }) },
  expected: ['Ticket Tags Overview', 'billing'],
  retryable: true,
})
