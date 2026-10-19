/**
 * Mount tests for DashboardView: the six tiles render the formatted numbers the API sent, the
 * loading state shows while the request is open, a refusal shows the API's sentence, and the
 * recent-activity and quick-link cards appear beside the tiles.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { nextTick } from 'vue'
import { flushPromises } from '@vue/test-utils'
import DashboardView from '@/views/DashboardView.vue'
import { mountRouted } from '../helpers/mount'
import { stubRoutes } from '../helpers/routes'
import { json, problem } from '../helpers/fetch'

/** Statistics as the dashboard endpoint returns them. */
const stats = {
  totalRevenue: 12500,
  monthlyRevenue: 900,
  activeServices: 42,
  overdueInvoices: 3,
  openTickets: 7,
  totalClients: 120,
}

/** Two events of the activity feed. */
const events = [
  { type: 'ClientRegistered', message: 'New client Ada', occurredAt: '2026-10-14T08:00:00Z', entityId: 7 },
  { type: 'DomainExpiring', message: 'example.com expires', occurredAt: '2026-10-14T09:00:00Z', entityId: 3 },
]

describe('DashboardView', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('renders a tile per statistic with money grouped and counts plain', async () => {
    stubRoutes({ 'GET /api/admin/dashboard/stats': () => json(stats) })
    const { wrapper } = await mountRouted(DashboardView, '/', '/')
    await flushPromises()

    expect(wrapper.get('[data-testid="dashboard-stat-totalRevenue"]').text()).toContain('$12,500')
    expect(wrapper.get('[data-testid="dashboard-stat-monthlyRevenue"]').text()).toContain('$900')
    expect(wrapper.get('[data-testid="dashboard-stat-activeServices"]').text()).toContain('42')
    expect(wrapper.get('[data-testid="dashboard-stat-totalClients"]').text()).toContain('120')
    expect(wrapper.get('[data-testid="dashboard-live"]').text()).toBe('Live')
  })

  it('shows the loading state until the statistics arrive', async () => {
    stubRoutes({ 'GET /api/admin/dashboard/stats': () => json(stats) })
    const { wrapper } = await mountRouted(DashboardView, '/', '/')
    await nextTick()

    expect(wrapper.find('[data-testid="dashboard-loading"]').exists()).toBe(true)

    await flushPromises()
    expect(wrapper.find('[data-testid="dashboard-loading"]').exists()).toBe(false)
  })

  it('shows the API sentence when the statistics are refused', async () => {
    stubRoutes({ 'GET /api/admin/dashboard/stats': () => problem('Not allowed.', 403) })
    const { wrapper } = await mountRouted(DashboardView, '/', '/')
    await flushPromises()

    expect(wrapper.get('[data-testid="dashboard-error"]').text()).toContain('Not allowed.')
    expect(wrapper.find('[data-testid="dashboard-stats"]').exists()).toBe(false)
  })

  it('previews the activity feed and links to the screens operators open most', async () => {
    stubRoutes({
      'GET /api/admin/dashboard/stats': () => json(stats),
      'GET /api/admin/dashboard/activity': () => json(events),
    })
    const { wrapper } = await mountRouted(DashboardView, '/', '/')
    await flushPromises()

    expect(wrapper.get('[data-testid="dashboard-activity"]').text()).toContain('New client Ada')
    expect(wrapper.get('[data-testid="dashboard-activity-1"]').text()).toContain('example.com expires')
    expect(wrapper.get('[data-testid="dashboard-link-orders"]').attributes('href')).toBe('/orders')
    expect(wrapper.get('[data-testid="dashboard-link-clients"]').text()).toBe('Clients')
  })

  it('leaves the activity card out while the feed is empty', async () => {
    stubRoutes({
      'GET /api/admin/dashboard/stats': () => json(stats),
      'GET /api/admin/dashboard/activity': () => json([]),
    })
    const { wrapper } = await mountRouted(DashboardView, '/', '/')
    await flushPromises()

    expect(wrapper.find('[data-testid="dashboard-activity"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="dashboard-quick-links"]').exists()).toBe(true)
  })
})
