/**
 * Tests of AgingInvoicesView: it mounts, shows its title and data, and shows the refusal sentence when the API
 * refuses.
 */
import { vi } from 'vitest'
import AgingInvoicesView from '@/views/AgingInvoicesView.vue'
import { json } from '../helpers/fetch'
import { runReportCase } from '../helpers/reportViews'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({
  name: 'AgingInvoicesView',
  view: AgingInvoicesView,
  routes: {
    'GET /reports/aging-invoices-summary': () => json({
      periods: [{ period: '1-30 days', amountsByCurrency: { USD: 10 } }],
      totals: { USD: 10 },
      currencies: ['USD'],
    }),
  },
  expected: ['Aging Invoices', '1-30 days', '$10.00'],
})
