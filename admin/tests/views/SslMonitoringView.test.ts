/**
 * Tests of SslMonitoringView: it mounts, shows its title and data, and shows the refusal sentence with Retry when the API
 * refuses, and runs its own actions.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import SslMonitoringView from '@/views/SslMonitoringView.vue'
import { json } from '../helpers/fetch'
import { mountView } from '../helpers/mount'
import { runReportCase } from '../helpers/reportViews'
import { requestsOf, stubRouteTable } from '../helpers/routes'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({
  name: 'SslMonitoringView',
  view: SslMonitoringView,
  routes: {
    'GET /reports/ssl-monitoring': () => json({
      groups: [{
        groupName: 'Active domains',
        rows: [{ domainName: 'beta.test', hasSsl: true, issuer: 'Test CA', expiresAt: '2030-01-01', lastUpdate: '2024-01-01', isActive: true }],
      }],
    }),
  },
  expected: ['SSL Certificate Monitoring', 'Active domains', 'beta.test', 'Valid', 'Test CA'],
  retryable: true,
})

describe('SslMonitoringView actions', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
    vi.restoreAllMocks()
  })

  it('reloads with inactive domains when the switch is pressed', async () => {
    const mock = stubRouteTable({ 'GET /reports/ssl-monitoring': () => json({ groups: [] }) })
    const wrapper = await mountView(SslMonitoringView)

    await wrapper.get('[data-testid="report-toggle-inactive"]').trigger('click')
    await flushPromises()

    expect(requestsOf(mock)).toEqual([
      'GET /reports/ssl-monitoring?includeInactive=false',
      'GET /reports/ssl-monitoring?includeInactive=true',
    ])
  })
})
