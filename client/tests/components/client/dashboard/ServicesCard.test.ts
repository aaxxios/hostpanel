/**
 * Unit tests for ClientDashboardServicesCard: the service list, its states and the
 * control-panel shortcut, against a real client store and a replaced management API.
 *
 * @module tests/components/client/dashboard/ServicesCard.test
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { nextTick } from 'vue'
import { mountClient } from '~/tests/helpers/mountClient'

/** The mocked single-sign-on call, re-created for every test. */
const fetchCpanelUrl = vi.fn()

vi.mock('~/stores/servicemanagement', () => ({
  /**
   * Stands in for the service-management store, whose transport is not under test here.
   *
   * @returns Only the action the card calls.
   */
  useServiceManagementStore: () => ({ fetchCpanelUrl })
}))

const { default: ServicesCard } = await import('~/components/client/dashboard/ServicesCard.vue')
const { useClientStore } = await import('~/stores/client')

/** A provisioned active service on a known host. */
const ACTIVE = { id: 1, name: 'Starter', domain: 'a.test', username: 'ada', status: 'Active', serverhostname: 'srv.test' }

/** A paid service that is not provisioned yet. */
const PENDING = { id: 2, name: 'Pro', domain: '', username: '', status: 'Pending', serverhostname: '' }

beforeEach(() => {
  fetchCpanelUrl.mockReset()
})

afterEach(() => {
  vi.restoreAllMocks()
})

describe('ClientDashboardServicesCard', () => {
  it('shows skeletons while loading', async () => {
    const wrapper = mountClient(ServicesCard)
    useClientStore().servicesLoading = true
    await nextTick()

    expect(wrapper.findAll('[data-testid="ui-skeleton"]')).toHaveLength(3)
  })

  it('invites the first order when there are no services', () => {
    const wrapper = mountClient(ServicesCard)

    expect(wrapper.find('[data-testid="dashboard-services-empty"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="dashboard-place-order"]').attributes('href')).toBe('/hosting')
  })

  it('links a provisioned service to its page and an unprovisioned one to setup', async () => {
    const wrapper = mountClient(ServicesCard)
    useClientStore().services = [ACTIVE, PENDING] as never
    await nextTick()

    expect(wrapper.find('[data-testid="dashboard-service-1"]').attributes('href')).toBe('/client/services/1')
    expect(wrapper.find('[data-testid="dashboard-service-2"]').attributes('href')).toBe('/client/services/2/setup')
  })

  it('shows the setup badge instead of a status for an unprovisioned service', async () => {
    const wrapper = mountClient(ServicesCard)
    useClientStore().services = [PENDING] as never
    await nextTick()

    const row = wrapper.find('[data-testid="dashboard-service-2"]')
    expect(row.text()).toContain('client.dashboard.needsSetup')
    expect(row.find('[data-testid="status-badge"]').exists()).toBe(false)
  })

  it('lists at most four services', async () => {
    const wrapper = mountClient(ServicesCard)
    useClientStore().services = [1, 2, 3, 4, 5].map((id) => ({ ...ACTIVE, id })) as never
    await nextTick()

    expect(wrapper.findAll('[data-testid^="dashboard-service-"]').filter((row) => /dashboard-service-\d+$/.test(row.attributes('data-testid') ?? ''))).toHaveLength(4)
  })

  it('offers the control panel only for an active service on a known host', async () => {
    const wrapper = mountClient(ServicesCard)
    useClientStore().services = [ACTIVE, { ...PENDING, id: 3, username: 'x', status: 'Suspended', serverhostname: 'srv.test' }] as never
    await nextTick()

    expect(wrapper.find('[data-testid="dashboard-service-cpanel-1"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="dashboard-service-cpanel-3"]').exists()).toBe(false)
  })

  it('opens the single-sign-on address in a new tab', async () => {
    const open = vi.spyOn(window, 'open').mockImplementation(() => null)
    fetchCpanelUrl.mockResolvedValue('https://sso.test/login')
    const wrapper = mountClient(ServicesCard)
    useClientStore().services = [ACTIVE] as never
    await nextTick()

    await wrapper.find('[data-testid="dashboard-service-cpanel-1"]').trigger('click')
    await flushPromises()

    expect(fetchCpanelUrl).toHaveBeenCalledWith('1')
    expect(open).toHaveBeenCalledWith('https://sso.test/login', '_blank', 'noopener')
  })

  it('falls back to the plain control-panel login when single sign-on gives no address', async () => {
    const open = vi.spyOn(window, 'open').mockImplementation(() => null)
    fetchCpanelUrl.mockResolvedValue(null)
    const wrapper = mountClient(ServicesCard)
    useClientStore().services = [ACTIVE] as never
    await nextTick()

    await wrapper.find('[data-testid="dashboard-service-cpanel-1"]').trigger('click')
    await flushPromises()

    expect(open).toHaveBeenCalledWith('https://srv.test:2083', '_blank', 'noopener')
  })
})
