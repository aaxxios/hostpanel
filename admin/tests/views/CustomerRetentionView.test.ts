/**
 * Tests of CustomerRetentionView: it mounts, shows its title and data, and shows the refusal sentence when the API
 * refuses, and runs its own actions.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import CustomerRetentionView from '@/views/CustomerRetentionView.vue'
import { json } from '../helpers/fetch'
import { mountView } from '../helpers/mount'
import { runReportCase } from '../helpers/reportViews'
import { requestsOf, stubRouteTable } from '../helpers/routes'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({
  name: 'CustomerRetentionView',
  view: CustomerRetentionView,
  routes: {
    'GET /reports/customer-retention': () => json({
      groups: [{
        groupName: 'Hosting',
        rows: [{ productName: 'Basic', billingCycle: 'Monthly', productCount: 2, avgDaysActive: 30, avgYearsMonthsActive: '0 years, 1 month' }],
      }],
    }),
  },
  expected: ['Average Customer Retention Time', 'Hosting', 'Basic', '0 years, 1 month'],
})

describe('CustomerRetentionView actions', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
    vi.restoreAllMocks()
  })

  it('reloads when the checkbox is toggled', async () => {
    const mock = stubRouteTable({ 'GET /reports/customer-retention': () => json({ groups: [] }) })
    const wrapper = await mountView(CustomerRetentionView)

    await wrapper.get('[data-testid="report-include-active"]').trigger('click')
    await flushPromises()

    expect(requestsOf(mock)).toEqual([
      'GET /reports/customer-retention?includeActive=true',
      'GET /reports/customer-retention?includeActive=false',
    ])
  })
})
