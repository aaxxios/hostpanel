/**
 * Tests for `pages/client/domains/[id].vue`: the panels, the pending lock and the switch state
 * that survives switching panels.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountPage, stubClientRuntime } from '~/tests/helpers/mountClient'
import DomainPage from '~/pages/client/domains/[id].vue'

/** The mocked endpoints the store reaches. */
const domainsApi = vi.hoisted(() => ({ fetchDomain: vi.fn(), setLock: vi.fn(), fetchNameservers: vi.fn() }))

vi.mock('~/composables/apis/useDomainsApi', () => ({ useDomainsApi: () => domainsApi }))
vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => ({}) }))
vi.mock('~/composables/apis/useBillingApi', () => ({ useBillingApi: () => ({}) }))
vi.mock('~/composables/apis/useCatalogApi', () => ({ useCatalogApi: () => ({}) }))
vi.mock('~/composables/apis/useCurrencyApi', () => ({ useCurrencyApi: () => ({}) }))

/**
 * Builds a domain.
 *
 * @param overrides - Fields to set.
 * @returns The domain.
 */
const domain = (overrides: Record<string, unknown> = {}) => ({
  id: 7, name: 'example.com', status: 'Active', registeredAt: '2025-01-01', expiresAt: '2099-01-01', nextDueDate: '2098-12-01',
  firstPaymentAmount: 15, recurringAmount: 12, registrationPeriod: 1, paymentMethod: 'card', priceCurrency: 'USD',
  autoRenew: false, isLocked: true, whoisPrivacy: false, ...overrides
})

describe('domain page', () => {
  beforeEach(() => {
    Object.values(domainsApi).forEach((fn) => fn.mockReset())
    domainsApi.fetchDomain.mockResolvedValue(domain())
    domainsApi.fetchNameservers.mockResolvedValue([])
    stubClientRuntime({ params: { id: '7' } })
  })

  it('reads this domain and opens on the overview', async () => {
    const wrapper = await mountPage(DomainPage)

    expect(domainsApi.fetchDomain).toHaveBeenCalledWith('7')
    expect(wrapper.find('[data-testid="domain-header"] h1').text()).toBe('example.com')
    expect(wrapper.find('[data-testid="domain-overview"]').exists()).toBe(true)
  })

  it.each([
    ['nameservers', 'domain-nameservers'],
    ['lock', 'domain-lock'],
    ['autorenew', 'domain-autorenew'],
    ['contact', 'domain-contact'],
    ['renew', 'domain-renew'],
  ])('opens the %s panel from the menu', async (tab, panel) => {
    const wrapper = await mountPage(DomainPage)

    await wrapper.find(`[data-testid="domain-tab-${tab}"], [data-testid="domain-action-${tab}"]`).trigger('click')

    expect(wrapper.find(`[data-testid="${panel}"]`).exists()).toBe(true)
    expect(wrapper.find('[data-testid="domain-overview"]').exists()).toBe(false)
  })

  it('explains a pending registration and locks the other panels', async () => {
    domainsApi.fetchDomain.mockResolvedValue(domain({ status: 'PendingRegistration' }))

    const wrapper = await mountPage(DomainPage)

    expect(wrapper.find('[data-testid="domain-pending"]').text()).toContain('client.domains.pendingRegistrationDesc')
    expect(wrapper.find('[data-testid="domain-tab-lock"]').attributes('disabled')).toBeDefined()
  })

  it('explains a pending transfer', async () => {
    domainsApi.fetchDomain.mockResolvedValue(domain({ status: 'PendingTransfer' }))

    const wrapper = await mountPage(DomainPage)

    expect(wrapper.find('[data-testid="domain-pending"]').text()).toContain('client.domains.pendingTransferDesc')
  })

  it('warns on the overview when the lock is off and the warning follows a change made on another panel', async () => {
    domainsApi.fetchDomain.mockResolvedValue(domain({ isLocked: false }))
    domainsApi.setLock.mockResolvedValue({})
    const wrapper = await mountPage(DomainPage)
    expect(wrapper.find('[data-testid="domain-unlock-warning"]').exists()).toBe(true)

    await wrapper.find('[data-testid="domain-tab-lock"]').trigger('click')
    await wrapper.find('[data-testid="domain-lock-row-toggle"]').trigger('click')
    await vi.waitFor(() => expect(domainsApi.setLock).toHaveBeenCalledWith('7', true))
    await wrapper.find('[data-testid="domain-tab-overview"]').trigger('click')

    expect(wrapper.find('[data-testid="domain-unlock-warning"]').exists()).toBe(false)
  })

  it('says the domain was not found when the read fails', async () => {
    domainsApi.fetchDomain.mockRejectedValue({ data: { detail: 'No such domain.' } })

    const wrapper = await mountPage(DomainPage)

    expect(wrapper.find('[data-testid="ui-empty-state"]').text()).toContain('client.domains.notFound')
  })
})
