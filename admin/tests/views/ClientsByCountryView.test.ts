/**
 * Tests of ClientsByCountryView: it mounts, shows its title and data, and shows the refusal sentence when the API
 * refuses.
 */
import { vi } from 'vitest'
import ClientsByCountryView from '@/views/ClientsByCountryView.vue'
import { json } from '../helpers/fetch'
import { runReportCase } from '../helpers/reportViews'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({
  name: 'ClientsByCountryView',
  view: ClientsByCountryView,
  routes: {
    'GET /reports/clients-by-country': () => json([{ country: 'AM', clientCount: 3, totalRevenue: 1 }]),
    'GET /reports/clients-by-city': () => json([{ city: 'Yerevan', country: 'AM', clientCount: 3 }]),
  },
  expected: ['Clients by Country', 'Armenia', 'Total Clients: 3'],
})
