/**
 * Tests of IncomeForecastView: it mounts, shows its title and data, and shows the refusal sentence when the API
 * refuses.
 */
import { vi } from 'vitest'
import IncomeForecastView from '@/views/IncomeForecastView.vue'
import { json } from '../helpers/fetch'
import { runReportCase } from '../helpers/reportViews'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({
  name: 'IncomeForecastView',
  view: IncomeForecastView,
  routes: {
    'GET /reports/income-forecast': () => json([{ month: 'Jan', monthly: 1, quarterly: 2, semiAnnual: 3, annual: 4, total: 10 }]),
  },
  expected: ['Income Forecast', 'Jan', '$10.00'],
})
