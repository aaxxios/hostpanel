/**
 * Unit tests for DomainAutoRenewPanel: the switch moves only when the API accepts the change.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import DomainAutoRenewPanel from '~/components/client/domains/DomainAutoRenewPanel.vue'
import { useDomainsStore } from '~/stores/domains'

/** The mocked domains endpoints the store reaches. */
const domainsApi = vi.hoisted(() => ({ setAutoRenew: vi.fn() }))

vi.mock('~/composables/apis/useDomainsApi', () => ({ useDomainsApi: () => domainsApi }))
vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => ({}) }))
vi.mock('~/composables/apis/useBillingApi', () => ({ useBillingApi: () => ({}) }))
vi.mock('~/composables/apis/useCatalogApi', () => ({ useCatalogApi: () => ({}) }))
vi.mock('~/composables/apis/useCurrencyApi', () => ({ useCurrencyApi: () => ({}) }))

describe('DomainAutoRenewPanel', () => {
  beforeEach(() => {
    domainsApi.setAutoRenew.mockReset()
  })

  it('shows the current state', () => {
    const on = mountClient(DomainAutoRenewPanel, { props: { enabled: true, domainId: '7' } })
    const off = mountClient(DomainAutoRenewPanel, { props: { enabled: false, domainId: '7' } })

    expect(on.text()).toContain('client.domains.autoRenewOn')
    expect(off.text()).toContain('client.domains.autoRenewOff')
  })

  it('asks the API and moves the switch when it accepts', async () => {
    domainsApi.setAutoRenew.mockResolvedValue({})
    const wrapper = mountClient(DomainAutoRenewPanel, { props: { enabled: false, domainId: '7' } })

    await wrapper.find('[data-testid="domain-autorenew-row-toggle"]').trigger('click')

    await vi.waitFor(() => expect(wrapper.emitted('update:enabled')).toEqual([[true]]))
    expect(domainsApi.setAutoRenew).toHaveBeenCalledWith('7', true)
  })

  it('leaves the switch where it was and shows the refusal when the API refuses', async () => {
    domainsApi.setAutoRenew.mockRejectedValue({ data: { detail: 'Registrar refused.' } })
    const wrapper = mountClient(DomainAutoRenewPanel, { props: { enabled: false, domainId: '7' } })

    await wrapper.find('[data-testid="domain-autorenew-row-toggle"]').trigger('click')
    await vi.waitFor(() => expect(useDomainsStore().toggleError).toBe('Registrar refused.'))
    await wrapper.vm.$nextTick()

    expect(wrapper.emitted('update:enabled')).toBeUndefined()
    expect(wrapper.find('[data-testid="domain-toggle-error"]').text()).toBe('Registrar refused.')
  })
})
