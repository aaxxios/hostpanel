/**
 * Unit tests for DomainEppCard: the lock warning, the request and the code or refusal shown.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import DomainEppCard from '~/components/client/domains/DomainEppCard.vue'
import { useDomainsStore } from '~/stores/domains'

/** The mocked domains endpoints the store reaches. */
const domainsApi = vi.hoisted(() => ({ requestEppCode: vi.fn() }))

vi.mock('~/composables/apis/useDomainsApi', () => ({ useDomainsApi: () => domainsApi }))
vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => ({}) }))
vi.mock('~/composables/apis/useBillingApi', () => ({ useBillingApi: () => ({}) }))
vi.mock('~/composables/apis/useCatalogApi', () => ({ useCatalogApi: () => ({}) }))
vi.mock('~/composables/apis/useCurrencyApi', () => ({ useCurrencyApi: () => ({}) }))

describe('DomainEppCard', () => {
  beforeEach(() => {
    domainsApi.requestEppCode.mockReset()
  })

  it('warns that a locked domain cannot be transferred', () => {
    const locked = mountClient(DomainEppCard, { props: { domainId: '7', lockEnabled: true } })
    const unlocked = mountClient(DomainEppCard, { props: { domainId: '7', lockEnabled: false } })

    expect(locked.find('[data-testid="domain-epp-lock-warning"]').exists()).toBe(true)
    expect(unlocked.find('[data-testid="domain-epp-lock-warning"]').exists()).toBe(false)
  })

  it('requests the code for this domain', async () => {
    domainsApi.requestEppCode.mockResolvedValue({ eppCode: 'AUTH-123' })
    const wrapper = mountClient(DomainEppCard, { props: { domainId: '7', lockEnabled: false } })

    await wrapper.find('[data-testid="domain-epp-request"]').trigger('click')

    await vi.waitFor(() => expect(wrapper.find('[data-testid="domain-epp-message"]').exists()).toBe(true))
    expect(domainsApi.requestEppCode).toHaveBeenCalledWith('7')
    expect(wrapper.find('[data-testid="domain-epp-message"]').text()).toBe('AUTH-123')
  })

  it('shows the refusal in the error colour', async () => {
    const wrapper = mountClient(DomainEppCard, { props: { domainId: '7', lockEnabled: false } })
    useDomainsStore().eppError = 'Not allowed.'
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="domain-epp-message"]').text()).toBe('Not allowed.')
    expect(wrapper.find('[data-testid="domain-epp-message"]').classes()).toContain('bg-red-50')
  })
})
