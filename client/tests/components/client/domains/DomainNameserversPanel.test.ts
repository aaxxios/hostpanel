/**
 * Unit tests for DomainNameserversPanel: filling the fields from the domain already loaded, the
 * default/custom choice, saving the non-empty fields in order and re-reading the domain after.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { mountClient } from '~/tests/helpers/mountClient'
import DomainNameserversPanel from '~/components/client/domains/DomainNameserversPanel.vue'
import { useDomainsStore } from '~/stores/domains'
import type { DomainDetail } from '~/types/domaindetail'
import type { DomainNameserver } from '~/types/domainnameserver'

/** The mocked domains endpoints the store reaches. */
const domainsApi = vi.hoisted(() => ({ setNameservers: vi.fn(), fetchDomain: vi.fn() }))

vi.mock('~/composables/apis/useDomainsApi', () => ({ useDomainsApi: () => domainsApi }))
vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => ({}) }))
vi.mock('~/composables/apis/useBillingApi', () => ({ useBillingApi: () => ({}) }))
vi.mock('~/composables/apis/useCatalogApi', () => ({ useCatalogApi: () => ({}) }))
vi.mock('~/composables/apis/useCurrencyApi', () => ({ useCurrencyApi: () => ({}) }))

/**
 * Builds a domain carrying the given nameservers in its management settings.
 *
 * @param hosts - The nameserver hostnames, in order.
 * @returns The domain.
 */
const domainWith = (hosts: string[]): DomainDetail => ({
  id: 7,
  name: 'example.com',
  management: {
    dnsManagement: false,
    emailForwarding: false,
    nameservers: hosts.map((host, index): DomainNameserver => ({ id: index + 1, host })),
    dnsRecords: [],
    emailForwardingRules: []
  }
} as unknown as DomainDetail)

/**
 * Mounts the panel with the domains store already holding the domain, as the page leaves it.
 *
 * @param hosts - The nameservers the loaded domain has.
 * @returns The mounted panel.
 */
const mountPanel = (hosts: string[]): ReturnType<typeof mountClient> => {
  const pinia = createPinia()
  setActivePinia(pinia)
  useDomainsStore().domain = domainWith(hosts)
  return mountClient(DomainNameserversPanel, { props: { domainId: '7' }, pinia })
}

/**
 * Reads the value of a nameserver field.
 *
 * @param wrapper - The mounted panel.
 * @param position - 1-based field number.
 * @returns The field's current value.
 */
const field = (wrapper: ReturnType<typeof mountClient>, position: number): string =>
  (wrapper.find(`[data-testid="domain-ns-${position}"]`).element as HTMLInputElement).value

describe('DomainNameserversPanel', () => {
  beforeEach(() => {
    domainsApi.setNameservers.mockReset()
    domainsApi.fetchDomain.mockReset()
  })

  it('fills the fields from the loaded domain without a request of its own and selects custom', () => {
    const wrapper = mountPanel(['ns1.example.net', 'ns2.example.net'])

    expect(field(wrapper, 1)).toBe('ns1.example.net')
    expect(field(wrapper, 2)).toBe('ns2.example.net')
    expect(domainsApi.fetchDomain).not.toHaveBeenCalled()
    expect(wrapper.find('[data-testid="domain-ns-custom"]').attributes('aria-checked')).toBe('true')
    // Five nameserver fields; the two radios are inputs too but not fields.
    expect(wrapper.findAll('input:not([type="radio"])')).toHaveLength(5)
  })

  it('selects the defaults and locks the fields when the domain has no nameservers', () => {
    const wrapper = mountPanel([])

    expect(wrapper.find('[data-testid="domain-ns-default"]').attributes('aria-checked')).toBe('true')
    expect(wrapper.find('[data-testid="domain-ns-1"]').attributes('disabled')).toBeDefined()
  })

  it('saves only the filled fields, in order, re-reads the domain and confirms', async () => {
    domainsApi.setNameservers.mockResolvedValue({})
    domainsApi.fetchDomain.mockResolvedValue(domainWith(['ns1.example.net', 'ns2.example.net', 'ns3.example.net']))
    const wrapper = mountPanel(['ns1.example.net', 'ns2.example.net'])

    await wrapper.find('[data-testid="domain-ns-3"]').setValue('ns3.example.net')
    await wrapper.find('form').trigger('submit')

    await vi.waitFor(() => expect(domainsApi.setNameservers).toHaveBeenCalledWith('7', ['ns1.example.net', 'ns2.example.net', 'ns3.example.net']))
    await vi.waitFor(() => expect(wrapper.find('[data-testid="ui-form-success"]').text()).toContain('client.domains.nsSuccess'))
    expect(domainsApi.fetchDomain).toHaveBeenCalledWith('7')
    expect(useDomainsStore().domain?.management.nameservers).toHaveLength(3)
  })

  it('shows the refusal and no confirmation when the save fails', async () => {
    domainsApi.setNameservers.mockRejectedValue({ data: { detail: 'Invalid nameserver.' } })
    const wrapper = mountPanel(['ns1.example.net'])

    await wrapper.find('form').trigger('submit')

    await vi.waitFor(() => expect(wrapper.find('[data-testid="ui-form-error"]').text()).toContain('Invalid nameserver.'))
    expect(wrapper.find('[data-testid="ui-form-success"]').exists()).toBe(false)
    expect(domainsApi.fetchDomain).not.toHaveBeenCalled()
  })
})
