/**
 * Tests of DiskUsageView: it mounts, shows its title and data, and shows the refusal sentence with Retry when the API
 * refuses, and runs its own actions.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import DiskUsageView from '@/views/DiskUsageView.vue'
import { json } from '../helpers/fetch'
import { mountView } from '../helpers/mount'
import { runReportCase } from '../helpers/reportViews'
import { requestsOf, stubRouteTable } from '../helpers/routes'

vi.mock('chart.js', async () => (await import('../helpers/chartJsMock')).chartJsMock)

runReportCase({
  name: 'DiskUsageView',
  view: DiskUsageView,
  routes: {
    'GET /reports/disk-usage': () => json({
      lastUpdated: null,
      servers: [{
        serverName: 'srv-1',
        rows: [{ clientName: 'Beta', domain: 'beta.test', diskUsage: '1 GB', diskLimit: '2 GB', diskPercent: 95, bwUsage: '1 GB', bwLimit: '5 GB', bwPercent: 10 }],
      }],
    }),
  },
  expected: ['Disk Space & Bandwidth Usage Summary', 'srv-1', 'beta.test', '95%'],
  retryable: true,
})

/** A disk usage report with one server and no accounts. */
const emptyDisk = { lastUpdated: null, servers: [{ serverName: 'srv-1', rows: [] }] }

describe('DiskUsageView actions', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
    vi.restoreAllMocks()
  })

  it('refreshes through a POST and shows the fresh figures', async () => {
    const mock = stubRouteTable({
      'GET /reports/disk-usage': () => json(emptyDisk),
      'POST /reports/disk-usage/update': () => json({ ...emptyDisk, servers: [{ serverName: 'srv-2', rows: [] }] }),
    })
    const wrapper = await mountView(DiskUsageView)

    await wrapper.get('[data-testid="report-update-now"]').trigger('click')
    await flushPromises()

    expect(requestsOf(mock)).toContain('POST /reports/disk-usage/update')
    expect(wrapper.text()).toContain('srv-2')
  })
})
