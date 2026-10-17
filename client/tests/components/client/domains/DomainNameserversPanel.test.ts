/**
 * Unit tests for DomainNameserversPanel: loading the current servers, the default/custom choice
 * and saving the non-empty fields in order.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import DomainNameserversPanel from '~/components/client/domains/DomainNameserversPanel.vue'

/** The mocked domains endpoints the store reaches. */
const domainsApi = vi.hoisted(() => ({ fetchNameservers: vi.fn(), setNameservers: vi.fn() }))

vi.mock('~/composables/apis/useDomainsApi', () => ({ useDomainsApi: () => domainsApi }))
vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => ({}) }))
vi.mock('~/composables/apis/useBillingApi', () => ({ useBillingApi: () => ({}) }))
vi.mock('~/composables/apis/useCatalogApi', () => ({ useCatalogApi: () => ({}) }))
vi.mock('~/composables/apis/useCurrencyApi', () => ({ useCurrencyApi: () => ({}) }))

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
    domainsApi.fetchNameservers.mockReset()
    domainsApi.setNameservers.mockReset()
  })

  it('fills the fields with the current nameservers and selects custom', async () => {
    domainsApi.fetchNameservers.mockResolvedValue([{ id: 1, host: 'ns1.example.net' }, { id: 2, host: 'ns2.example.net' }])
    const wrapper = mountClient(DomainNameserversPanel, { props: { domainId: '7' } })

    await vi.waitFor(() => expect(field(wrapper, 1)).toBe('ns1.example.net'))

    expect(field(wrapper, 2)).toBe('ns2.example.net')
    expect(wrapper.find('[data-testid="domain-ns-custom"]').attributes('aria-checked')).toBe('true')
    // Five nameserver fields; the two radios are inputs too but not fields.
    expect(wrapper.findAll('input:not([type="radio"])')).toHaveLength(5)
  })

  it('selects the defaults and locks the fields when the domain has no nameservers', async () => {
    domainsApi.fetchNameservers.mockResolvedValue([])
    const wrapper = mountClient(DomainNameserversPanel, { props: { domainId: '7' } })

    await vi.waitFor(() => expect(wrapper.find('[data-testid="domain-ns-default"]').attributes('aria-checked')).toBe('true'))

    expect(wrapper.find('[data-testid="domain-ns-1"]').attributes('disabled')).toBeDefined()
  })

  it('saves only the filled fields, in order, and confirms', async () => {
    domainsApi.fetchNameservers.mockResolvedValue([{ id: 1, host: 'ns1.example.net' }, { id: 2, host: 'ns2.example.net' }])
    domainsApi.setNameservers.mockResolvedValue({})
    const wrapper = mountClient(DomainNameserversPanel, { props: { domainId: '7' } })
    await vi.waitFor(() => expect(field(wrapper, 1)).toBe('ns1.example.net'))

    await wrapper.find('[data-testid="domain-ns-3"]').setValue('ns3.example.net')
    await wrapper.find('form').trigger('submit')

    await vi.waitFor(() => expect(domainsApi.setNameservers).toHaveBeenCalledWith('7', ['ns1.example.net', 'ns2.example.net', 'ns3.example.net']))
    await vi.waitFor(() => expect(wrapper.find('[data-testid="ui-form-success"]').text()).toContain('client.domains.nsSuccess'))
  })

  it('shows the refusal and no confirmation when the save fails', async () => {
    domainsApi.fetchNameservers.mockResolvedValue([{ id: 1, host: 'ns1.example.net' }])
    domainsApi.setNameservers.mockRejectedValue({ data: { detail: 'Invalid nameserver.' } })
    const wrapper = mountClient(DomainNameserversPanel, { props: { domainId: '7' } })
    await vi.waitFor(() => expect(field(wrapper, 1)).toBe('ns1.example.net'))

    await wrapper.find('form').trigger('submit')

    await vi.waitFor(() => expect(wrapper.find('[data-testid="ui-form-error"]').text()).toContain('Invalid nameserver.'))
    expect(wrapper.find('[data-testid="ui-form-success"]').exists()).toBe(false)
  })
})
