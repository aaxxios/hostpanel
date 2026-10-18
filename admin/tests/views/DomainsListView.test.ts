/**
 * Mount tests for DomainsListView: rows render with status, price and links to the owning
 * client's detail page, the search is sent to the API after a pause, and a refused load or an
 * empty result shows the right state.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import DomainsListView from '@/views/DomainsListView.vue'
import { mountRouted } from '../helpers/mount'
import { stubRoutes } from '../helpers/routes'
import { json, problem } from '../helpers/fetch'

/** One active domain as the list endpoint returns it. */
const domain = {
  id: 4,
  clientId: 9,
  clientName: 'Grace Hopper',
  name: 'example.com',
  regPeriod: '1 Year',
  registrar: null,
  recurringAmount: 12.99,
  priceCurrency: 'USD',
  nextDueDate: '2027-01-01T00:00:00Z',
  expiresAt: '2027-01-01T00:00:00Z',
  autoRenew: true,
  status: 'Active',
}

/**
 * Mounts the list and waits for its first load.
 *
 * @returns The wrapper.
 */
const mountList = async () => {
  const { wrapper } = await mountRouted(DomainsListView, '/domains', '/domains')
  await flushPromises()
  return wrapper
}

describe('DomainsListView', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('renders a row with the domain, price, auto-renew and links under the client', async () => {
    stubRoutes({ 'GET /api/domains': () => json({ items: [domain], totalCount: 1 }) })
    const wrapper = await mountList()

    const row = wrapper.get('[data-testid="domains-row-4"]')
    expect(row.text()).toContain('example.com')
    expect(row.text()).toContain('Grace Hopper')
    expect(row.text()).toContain('$12.99 USD')
    expect(row.text()).toContain('Yes')
    expect(wrapper.get('[data-testid="domains-name-4"]').attributes('href')).toBe('/clients/9/domains/4')
    expect(wrapper.get('[data-testid="domains-view-4"]').attributes('href')).toBe('/clients/9/domains/4')
    expect(wrapper.get('[data-testid="domains-client-4"]').attributes('href')).toBe('/clients/9')
  })

  it('sends the typed search to the API after the pause', async () => {
    const mock = stubRoutes({ 'GET /api/domains': () => json({ items: [domain], totalCount: 1 }) })
    const wrapper = await mountList()

    await wrapper.get('[data-testid="domains-search"]').setValue('exam')
    await new Promise((resolve) => setTimeout(resolve, 450))
    await flushPromises()

    const urls = mock.mock.calls.map(([url]) => url as string)
    expect(urls.some((url) => url.includes('search=exam'))).toBe(true)
  })

  it('shows the empty state, and the search hint once something was typed', async () => {
    stubRoutes({ 'GET /api/domains': () => json({ items: [], totalCount: 0 }) })
    const wrapper = await mountList()

    expect(wrapper.get('[data-testid="domains-empty"]').text()).toContain('No domains found')

    await wrapper.get('[data-testid="domains-search"]').setValue('zzz')
    expect(wrapper.get('[data-testid="domains-empty"]').text()).toContain('No domains match your search')
  })

  it('shows the API sentence when the load is refused', async () => {
    stubRoutes({ 'GET /api/domains': () => problem('Not allowed.', 403) })
    const wrapper = await mountList()

    expect(wrapper.get('[data-testid="domains-error"]').text()).toContain('Not allowed.')
  })
})
