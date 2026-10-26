/**
 * Tests for `pages/client/dashboard.vue` against the real client and content stores: the data
 * the page loads, what the cards render from it, the load-error and no-profile notices and the
 * manual refresh. Only the API composables are replaced.
 *
 * @module tests/pages/client/dashboard.test
 */

import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountAuthPage, stubAuthRuntime } from '~/tests/helpers/authScreens'
import { Permission } from '~/composables/usePermissions'

/** The mocked identity call, re-created for every test. */
const fetchMe = vi.fn()

/** The mocked services call, re-created for every test. */
const fetchServices = vi.fn()

/** The mocked invoices call, re-created for every test. */
const fetchInvoices = vi.fn()

/** The mocked domains call, re-created for every test. */
const fetchDomains = vi.fn()

/** The mocked tickets call, re-created for every test. */
const fetchTickets = vi.fn()

/** The mocked announcements call, re-created for every test. */
const fetchRecentAnnouncements = vi.fn()

vi.mock('~/composables/apis/useClientApi', () => ({
  /**
   * Stands in for the real API composable so the client store runs without transport.
   *
   * @returns The endpoints the dashboard loads.
   */
  useClientApi: () => ({ fetchMe, fetchServices, fetchInvoices, fetchDomains, fetchTickets })
}))

vi.mock('~/composables/apis/useContentApi', () => ({
  /**
   * Stands in for the real API composable so the content store runs without transport.
   *
   * @returns The endpoint the news card loads.
   */
  useContentApi: () => ({ fetchRecentAnnouncements })
}))

const { default: DashboardPage } = await import('~/pages/client/dashboard.vue')
const { useDashboardSummary } = await import('~/composables/useDashboardSummary')

beforeEach(() => {
  stubAuthRuntime()
  vi.stubGlobal('useDashboardSummary', useDashboardSummary)
  for (const call of [fetchMe, fetchServices, fetchInvoices, fetchDomains, fetchTickets, fetchRecentAnnouncements]) {
    call.mockReset()
  }
  fetchMe.mockResolvedValue({ firstname: 'Ada', lastname: 'Lovelace', email: 'ada@example.com', permissions: Permission.All })
  fetchServices.mockResolvedValue([{ id: 1, name: 'Starter', domain: 'a.test', username: 'ada', status: 'Active', serverhostname: '' }])
  fetchInvoices.mockResolvedValue([{ id: 7, status: 'Unpaid', dueDate: '2026-11-01', total: 25, currency: 'USD' }])
  fetchDomains.mockResolvedValue([{ id: 3, name: 'a.test', status: 'Active', expiresAt: '2030-01-01', nextDueDate: '', registeredAt: '' }])
  fetchTickets.mockResolvedValue([{ id: 9, subject: 'Help', departmentName: 'Support', status: 'Open' }])
  fetchRecentAnnouncements.mockResolvedValue([{ id: 5, title: 'Maintenance', date: '2026-10-01' }])
})

describe('client dashboard page', () => {
  it('shows the title and subtitle in a header the e2e flow can wait for', async () => {
    const wrapper = await mountAuthPage(DashboardPage)

    const header = wrapper.find('[data-testid="dashboard-title"]')
    expect(header.text()).toContain('client.dashboard.title')
    expect(header.text()).toContain('client.dashboard.subtitle')
  })

  it('loads every section once and renders each card from it', async () => {
    const wrapper = await mountAuthPage(DashboardPage)

    expect(fetchServices).toHaveBeenCalledTimes(1)
    expect(fetchInvoices).toHaveBeenCalledTimes(1)
    expect(fetchDomains).toHaveBeenCalledTimes(1)
    expect(fetchTickets).toHaveBeenCalledTimes(1)
    expect(wrapper.find('[data-testid="dashboard-service-1"]').text()).toContain('Starter')
    expect(wrapper.find('[data-testid="dashboard-invoice-7"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="dashboard-domain-3"]').text()).toContain('a.test')
    expect(wrapper.find('[data-testid="dashboard-ticket-9"]').text()).toContain('Help')
    expect(wrapper.find('[data-testid="dashboard-news-5"]').text()).toContain('Maintenance')
  })

  it('counts the sections in the stat tiles', async () => {
    const wrapper = await mountAuthPage(DashboardPage)

    expect(wrapper.find('[data-testid="dashboard-stat-services"] [data-testid="ui-stat-tile-value"]').text()).toBe('1')
    expect(wrapper.find('[data-testid="dashboard-stat-invoices"] [data-testid="ui-stat-tile-value"]').text()).toBe('1')
  })

  it('flags the unpaid invoice in the banners', async () => {
    const wrapper = await mountAuthPage(DashboardPage)

    expect(wrapper.find('[data-testid="dashboard-alert-unpaid"]').exists()).toBe(true)
  })

  it('shows each failed section once as a load error', async () => {
    fetchServices.mockRejectedValue({ data: { detail: 'The billing system is down.' } })
    fetchInvoices.mockRejectedValue({ data: { detail: 'The billing system is down.' } })

    const wrapper = await mountAuthPage(DashboardPage)

    const alert = wrapper.find('[data-testid="dashboard-load-errors"]')
    expect(alert.text().match(/The billing system is down\./g)).toHaveLength(1)
  })

  it('shows the no-profile notice instead of load errors for a staff account', async () => {
    const refusal = { data: { type: 'CLIENT_PROFILE_NOT_FOUND', detail: 'No client record.' } }
    fetchMe.mockRejectedValue(refusal)
    fetchServices.mockRejectedValue(refusal)
    fetchInvoices.mockRejectedValue(refusal)
    fetchDomains.mockRejectedValue(refusal)
    fetchTickets.mockRejectedValue(refusal)

    const wrapper = await mountAuthPage(DashboardPage)

    expect(wrapper.find('[data-testid="no-profile-notice"]').text()).toContain('No client record.')
    expect(wrapper.find('[data-testid="dashboard-load-errors"]').exists()).toBe(false)
  })

  it('reloads every section from the API when refresh is pressed', async () => {
    const wrapper = await mountAuthPage(DashboardPage)

    await wrapper.find('[data-testid="dashboard-refresh"]').trigger('click')
    await flushPromises()

    expect(fetchServices).toHaveBeenCalledTimes(2)
    expect(fetchInvoices).toHaveBeenCalledTimes(2)
    expect(fetchDomains).toHaveBeenCalledTimes(2)
    expect(fetchTickets).toHaveBeenCalledTimes(2)
  })
})

describe('client dashboard page permissions', () => {
  it('neither asks for nor shows the sections the person has no permission for', async () => {
    fetchMe.mockResolvedValue({ firstname: 'Ada', lastname: 'Lovelace', email: 'ada@example.com', permissions: Permission.ViewDomains })

    const wrapper = await mountAuthPage(DashboardPage)

    expect(fetchDomains).toHaveBeenCalledTimes(1)
    expect(fetchServices).not.toHaveBeenCalled()
    expect(fetchInvoices).not.toHaveBeenCalled()
    expect(fetchTickets).not.toHaveBeenCalled()
    expect(wrapper.find('[data-testid="dashboard-domain-3"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="dashboard-service-1"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="dashboard-invoice-7"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="dashboard-ticket-9"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="dashboard-register-domain"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="dashboard-load-errors"]').exists()).toBe(false)
  })

  it('offers the domain search card to a person who may place orders', async () => {
    fetchMe.mockResolvedValue({ firstname: 'Ada', lastname: 'Lovelace', email: 'ada@example.com', permissions: Permission.PlaceNewOrders })

    const wrapper = await mountAuthPage(DashboardPage)

    expect(wrapper.find('[data-testid="dashboard-register-domain"]').exists()).toBe(true)
  })
})
