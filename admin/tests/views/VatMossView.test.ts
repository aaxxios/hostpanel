/**
 * Tests of VatMossView: it mounts, shows its title and data, and shows the refusal sentence when the API
 * refuses.
 */
import { vi } from 'vitest'
import VatMossView from '@/views/VatMossView.vue'
import { json } from '../helpers/fetch'
import { runReportCase } from '../helpers/reportViews'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({
  name: 'VatMossView',
  view: VatMossView,
  routes: {
    'GET /reports/vat-moss': () => json({
      periodLabel: 'Q1 2024',
      rows: [{ countryName: 'Germany', countryCode: 'DE', vatRate: 19, numberOfInvoices: 2, totalExclVat: 100, totalVatCollected: 19, currency: 'EUR' }],
    }),
  },
  expected: ['VAT MOSS Settlement Data', 'Q1 2024', 'Germany', '19%'],
})
