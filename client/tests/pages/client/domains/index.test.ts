/**
 * Tests for `pages/client/domains/index.vue`: the table, the tabs, the search, the paging and
 * the expiring-soon badge.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountPage, stubClientRuntime } from '~/tests/helpers/mountClient'
import DomainsPage from '~/pages/client/domains/index.vue'
import type { ClientDomain } from '~/types/clientdomain'
import { DomainStatus } from '~/types/domainstatus'

/** The mocked client endpoints the store reaches. */
const clientApi = vi.hoisted(() => ({ fetchDomains: vi.fn() }))

vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => clientApi }))

/**
 * Builds a domain with the fields the table reads.
 *
 * @param id - Domain id.
 * @param status - Domain status.
 * @param expiresAt - Expiry date.
 * @returns A domain.
 */
const domain = (id: number, status: string, expiresAt = '2099-01-01T00:00:00Z'): ClientDomain =>
  ({ id, name: `site${id}.com`, status, registeredAt: '2025-01-01', expiresAt } as ClientDomain)

/**
 * Lists the names of the domains the table shows.
 *
 * @param wrapper - The mounted page.
 * @returns The names, in order.
 */
const names = (wrapper: Awaited<ReturnType<typeof mountPage>>): string[] =>
  wrapper.findAll('[data-testid^="domain-row-"]').map((row) => row.find('td').text())

describe('domain list page', () => {
  beforeEach(() => {
    clientApi.fetchDomains.mockReset()
    stubClientRuntime()
  })

  it('lists the domains with a manage link each', async () => {
    clientApi.fetchDomains.mockResolvedValue([domain(1, DomainStatus.Active), domain(2, DomainStatus.Expired)])

    const wrapper = await mountPage(DomainsPage)

    expect(names(wrapper)).toEqual(['site1.com', 'site2.com'])
    expect(wrapper.find('[data-testid="domain-manage-2"]').attributes('href')).toBe('/client/domains/2')
  })

  it('filters to the active and to the expired domains', async () => {
    clientApi.fetchDomains.mockResolvedValue([domain(1, DomainStatus.Active), domain(2, DomainStatus.Expired), domain(3, DomainStatus.Active)])
    const wrapper = await mountPage(DomainsPage)

    await wrapper.find('[data-testid="ui-tab-current"]').trigger('click')
    expect(names(wrapper)).toEqual(['site1.com', 'site3.com'])

    await wrapper.find('[data-testid="ui-tab-finished"]').trigger('click')
    expect(names(wrapper)).toEqual(['site2.com'])
  })

  it('narrows the list by the search text, ignoring case', async () => {
    clientApi.fetchDomains.mockResolvedValue([domain(1, DomainStatus.Active), domain(22, DomainStatus.Active)])
    const wrapper = await mountPage(DomainsPage)

    await wrapper.find('[data-testid="domains-search"]').setValue('SITE22')

    expect(names(wrapper)).toEqual(['site22.com'])
  })

  it('offers a search of new domains only when the list is empty without a query', async () => {
    clientApi.fetchDomains.mockResolvedValue([])
    const wrapper = await mountPage(DomainsPage)

    expect(wrapper.find('[data-testid="domains-search-domains"]').attributes('href')).toBe('/domains')

    await wrapper.find('[data-testid="domains-search"]').setValue('nothing')
    expect(wrapper.find('[data-testid="domains-search-domains"]').exists()).toBe(false)
  })

  it('shows the first ten and pages the rest', async () => {
    clientApi.fetchDomains.mockResolvedValue(Array.from({ length: 12 }, (_, index) => domain(index + 1, DomainStatus.Active)))

    const wrapper = await mountPage(DomainsPage)

    expect(names(wrapper)).toHaveLength(10)
    expect(wrapper.find('[data-testid="ui-pagination-next"]').exists()).toBe(true)
  })

  it('flags a domain that expires within 30 days', async () => {
    const soon = new Date(Date.now() + 10 * 86_400_000).toISOString()
    clientApi.fetchDomains.mockResolvedValue([domain(1, DomainStatus.Active, soon), domain(2, DomainStatus.Active)])

    const wrapper = await mountPage(DomainsPage)

    expect(wrapper.find('[data-testid="domain-row-1"]').text()).toContain('client.domains.expiringSoon')
    expect(wrapper.find('[data-testid="domain-row-2"]').text()).not.toContain('client.domains.expiringSoon')
  })

  it('shows the failure, not the empty state, when the read failed', async () => {
    clientApi.fetchDomains.mockRejectedValue({ data: { detail: 'Registrar is down.' } })

    const wrapper = await mountPage(DomainsPage)

    expect(wrapper.find('[data-testid="domains-error"]').text()).toBe('Registrar is down.')
  })
})
