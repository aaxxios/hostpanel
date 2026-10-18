/**
 * Smoke tests for NetworkIssuesView: the Open tab loads first, switching tab reloads from page 1,
 * and the delete confirmation, empty and error states, through the real store and transport.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import NetworkIssuesView from '@/views/NetworkIssuesView.vue'
import { NetworkIssuePriority, NetworkIssueStatus, NetworkIssueType } from '@/types/networkissue'
import { mountView } from '../helpers/mount'
import { problem, stubFetch, callOf } from '../helpers/fetch'
import { stubPathRoutes } from '../helpers/routes'

/** One network issue as the list endpoint returns it. */
const issue = {
  id: 2,
  title: 'Packet loss',
  type: NetworkIssueType.Server,
  server: 'srv-1',
  priority: NetworkIssuePriority.Critical,
  status: NetworkIssueStatus.Investigating,
  startDate: '2026-10-01T08:00:00Z',
  endDate: null,
  description: 'Investigating',
  createdAt: '2026-10-01T08:00:00Z',
}

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('NetworkIssuesView', () => {
  it('loads the Open tab first and lists the issue with its priority and status', async () => {
    const mock = stubPathRoutes([{ path: '/network-issues', body: { items: [issue], totalCount: 1 } }])

    const wrapper = await mountView(NetworkIssuesView)
    const row = wrapper.get('[data-testid="network-issues-row-2"]')

    expect(callOf(mock).url).toBe('/api/network-issues?page=1&pageSize=20&status=Open')
    expect(row.text()).toContain('Packet loss')
    expect(row.text()).toContain('Critical')
    expect(row.text()).toContain('Investigating')
  })

  it('reloads with the chosen tab', async () => {
    const mock = stubPathRoutes([{ path: '/network-issues', body: { items: [], totalCount: 0 } }])
    const wrapper = await mountView(NetworkIssuesView)

    await wrapper.get('[data-testid="network-issues-tab-Resolved"]').trigger('click')
    await flushPromises()

    expect(callOf(mock, 1).url).toBe('/api/network-issues?page=1&pageSize=20&status=Resolved')
  })

  it('deletes after confirmation', async () => {
    const mock = stubPathRoutes([{ path: '/network-issues', body: { items: [issue], totalCount: 1 } }])
    const wrapper = await mountView(NetworkIssuesView)

    await wrapper.get('[data-testid="network-issues-2-delete"]').trigger('click')
    await wrapper.get('[data-testid="dialog-confirm"]').trigger('click')
    await flushPromises()

    expect(callOf(mock, 1)).toMatchObject({ url: '/api/network-issues/2', method: 'DELETE' })
  })

  it('shows the empty state for a tab with no issues', async () => {
    stubPathRoutes([{ path: '/network-issues', body: { items: [], totalCount: 0 } }])

    const wrapper = await mountView(NetworkIssuesView)

    expect(wrapper.get('[data-testid="network-issues-table"]').text()).toContain('No Records Found')
  })

  it('shows the API sentence when the list is refused', async () => {
    stubFetch(problem('Not allowed.', 403))

    const wrapper = await mountView(NetworkIssuesView)

    expect(wrapper.get('[data-testid="network-issues-error"]').text()).toBe('Not allowed.')
  })
})
