/**
 * Tests of DomainsReportView: it mounts, shows its title and data, and shows the refusal sentence when the API
 * refuses.
 */
import { vi } from 'vitest'
import DomainsReportView from '@/views/DomainsReportView.vue'
import { json } from '../helpers/fetch'
import { runReportCase } from '../helpers/reportViews'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({
  name: 'DomainsReportView',
  view: DomainsReportView,
  routes: { 'GET /reports/domains': () => json({ items: [], totalCount: 0 }) },
  expected: ['Domains', '0 domain(s) found', 'No domains found.'],
})
