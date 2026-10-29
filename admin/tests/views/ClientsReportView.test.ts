/**
 * Tests of ClientsReportView: it mounts, shows its title and data, and shows the refusal sentence when the API
 * refuses.
 */
import { vi } from 'vitest'
import ClientsReportView from '@/views/ClientsReportView.vue'
import { json } from '../helpers/fetch'
import { runReportCase } from '../helpers/reportViews'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({
  name: 'ClientsReportView',
  view: ClientsReportView,
  routes: { 'GET /reports/clients': () => json({ items: [], pageNumber: 1, pageSize: 50, totalCount: 0, totalPages: 0 }) },
  expected: ['Clients', '0 client(s) found', 'No clients found. Click Filter to load results.'],
  retryable: true,
})
