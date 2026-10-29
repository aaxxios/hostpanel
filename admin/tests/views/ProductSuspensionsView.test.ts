/**
 * Tests of ProductSuspensionsView: it mounts, shows its title and data, and shows the refusal sentence with Retry when the API
 * refuses.
 */
import { vi } from 'vitest'
import ProductSuspensionsView from '@/views/ProductSuspensionsView.vue'
import { json } from '../helpers/fetch'
import { runReportCase } from '../helpers/reportViews'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({
  name: 'ProductSuspensionsView',
  view: ProductSuspensionsView,
  routes: {
    'GET /reports/product-suspensions': () => json([
      { serviceId: 1, clientName: 'Beta', productName: 'Basic', domain: null, nextDueDate: null, suspendReason: 'Unpaid invoice' },
    ]),
  },
  expected: ['Product Suspensions', 'Beta', 'Unpaid invoice'],
  retryable: true,
})
