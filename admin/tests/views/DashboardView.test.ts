/**
 * Mount tests for DashboardView: the six tiles render the formatted numbers the API sent, the
 * loading state shows while the request is open, and a refusal shows the API's sentence.
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
})
