/**
 * Tests of SalesTaxView: it mounts, shows its title and data, and shows the refusal sentence when the API
 * refuses.
 */
import { vi } from 'vitest'
import SalesTaxView from '@/views/SalesTaxView.vue'
import { json } from '../helpers/fetch'
import { runReportCase } from '../helpers/reportViews'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({
  name: 'SalesTaxView',
  view: SalesTaxView,
  routes: {
    'GET /reports/sales-tax': () => json({
      totalInvoices: 1, totalInvoiced: 11, taxLevel1Liability: 1, taxLevel2Liability: 0,
      rows: [{ id: 3, clientName: 'Beta', invoiceDate: '2024-01-01', datePaid: null, subTotal: 10, tax: 1, credit: 0, total: 11 }],
    }),
  },
  expected: ['Sales Tax Liability', 'Beta', '$11.00 USD'],
})
