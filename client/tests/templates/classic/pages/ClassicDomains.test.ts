/**
 * Unit tests for ClassicDomains: the availability search and its three outcomes, adding a
 * registration to the cart, the TLD price table with its category filter and quick search.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { at } from '~/tests/helpers/at'
import { mountPage, stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import ClassicDomains from '~/templates/classic/pages/ClassicDomains.vue'
import { useCartStore } from '~/stores/cart'
import type { BillingCurrency } from '~/types/currency'
import type { TldPricing } from '~/types/tldpricing'
import { CartItemType } from '~/types/cartitemtype'
import { DomainOrderAction } from '~/types/domainorderaction'
import { DomainAvailabilityStatus } from '~/types/domainavailabilitystatus'
import { ProductType } from '~/types/producttype'

/** The mocked catalogue endpoints, re-created for every test. */
const api = { checkDomain: vi.fn(), fetchProducts: vi.fn(), fetchTldPricing: vi.fn() }

/** The currency the stood-in currency endpoint answers with. */
const usd: BillingCurrency = { code: 'USD', numeric: '840', prefix: '$', suffix: '', decimals: 2, isBase: true }

vi.mock('~/composables/apis/useCatalogApi', () => ({
  /**
   * Stands in for the catalogue API composable so the stores run without transport.
   *
   * @returns The mocked endpoints.
   */
  useCatalogApi: () => api
}))

vi.mock('~/composables/apis/useCurrencyApi', () => ({
  /**
   * Stands in for the currency API composable.
   *
   * @returns Only the endpoint the page's store reaches for.
   */
  useCurrencyApi: () => ({ loadBillingCurrencies: () => Promise.resolve([usd]) })
}))

vi.mock('~/composables/apis/useDomainsApi', () => ({
  /**
   * Stands in for the domains API composable; the search page calls none of its endpoints.
   *
   * @returns An empty set of endpoints.
   */
  useDomainsApi: () => ({})
}))

vi.mock('~/composables/apis/useBillingApi', () => ({
  /**
   * Stands in for the billing API composable; the search page calls none of its endpoints.
   *
   * @returns An empty set of endpoints.
   */
  useBillingApi: () => ({})
}))

vi.mock('~/composables/apis/useClientApi', () => ({
  /**
   * Stands in for the client API composable; the search page calls none of its endpoints.
   *
   * @returns An empty set of endpoints.
   */
  useClientApi: () => ({})
}))

/** The price table the catalogue answers with. */
const pricing: TldPricing = {
  currency: { code: 'USD', prefix: '$' },
  pricing: [
    { tld: 'com', sellCurrency: 'USD', register: [{ years: 1, amount: 10 }], transfer: [{ years: 1, amount: 9 }], renew: [{ years: 1, amount: 11 }], categories: ['popular'] },
    { tld: 'dev', sellCurrency: 'USD', register: [{ years: 1, amount: 15 }], transfer: [{ years: 1, amount: 14 }], renew: [{ years: 1, amount: 16 }], categories: ['technology'] },
  ],
}

/**
 * Mounts the page and lets the currencies, prices and domain product load.
 *
 * @param query Route query, e.g. a `domain` to search at once.
 * @returns The wrapper.
 */
const mountDomains = async (query: Record<string, string> = {}) => {
  stubPublicPageRuntime({ query })
  const wrapper = await mountPage(ClassicDomains)
  await flushPromises()
  return wrapper
}

/**
 * Types a domain into the search box and submits it.
 *
 * @param wrapper The mounted page.
 * @param domain The text to search for.
 */
const search = async (wrapper: Awaited<ReturnType<typeof mountDomains>>, domain: string) => {
  await wrapper.find('[data-testid="search-bar-input"]').setValue(domain)
  await wrapper.find('[data-testid="search-bar-submit"]').trigger('click')
  await flushPromises()
}

describe('ClassicDomains', () => {
  beforeEach(() => {
    api.checkDomain.mockReset()
    api.fetchProducts.mockReset()
    api.fetchProducts.mockResolvedValue([{ id: 99, type: ProductType.Domain }])
    api.fetchTldPricing.mockReset()
    api.fetchTldPricing.mockResolvedValue(pricing)
    window.localStorage.clear()
  })

  it('renders the hero heading and the register and transfer tabs', async () => {
    const wrapper = await mountDomains()

    expect(wrapper.find('[data-testid="classic-domains-title"]').text()).toContain('domains.title')
    expect(wrapper.find('[data-testid="classic-domains-tab-transfer"]').attributes('href')).toBe('/en/domains/transfer')
  })

  it('does not search an empty box', async () => {
    const wrapper = await mountDomains()

    await wrapper.find('[data-testid="search-bar-submit"]').trigger('click')

    expect(api.checkDomain).not.toHaveBeenCalled()
  })

  it('refuses a name without an extension before asking the server', async () => {
    const wrapper = await mountDomains()

    await search(wrapper, 'example')

    expect(api.checkDomain).not.toHaveBeenCalled()
    expect(wrapper.find('[data-testid="classic-domains-error"]').text()).toBe('domains.invalidDomain')
  })

  it('shows an available domain with a register button', async () => {
    api.checkDomain.mockResolvedValue({ domain: 'example.com', available: true, status: DomainAvailabilityStatus.Available })
    const wrapper = await mountDomains()

    await search(wrapper, 'example.com')

    const result = wrapper.find('[data-testid="classic-domains-availability"]')
    expect(result.text()).toContain('example.com')
    expect(result.text()).toContain('domains.available')
    expect(wrapper.find('[data-testid="classic-domains-register"]').exists()).toBe(true)
  })

  it('adds the registration to the cart at the one-year price', async () => {
    api.checkDomain.mockResolvedValue({ domain: 'example.com', available: true, status: DomainAvailabilityStatus.Available })
    const wrapper = await mountDomains()
    await search(wrapper, 'example.com')

    await wrapper.find('[data-testid="classic-domains-register"]').trigger('click')

    expect(useCartStore().items[0]).toMatchObject({
      pid: 99,
      name: 'example.com',
      domain: 'example.com',
      itemType: CartItemType.Domain,
      domainAction: DomainOrderAction.Register,
      tld: 'com',
      years: 1,
      amount: 10,
      currency: 'USD',
    })
  })

  it('disables the register button once the domain is in the cart', async () => {
    api.checkDomain.mockResolvedValue({ domain: 'example.com', available: true, status: DomainAvailabilityStatus.Available })
    const wrapper = await mountDomains()
    await search(wrapper, 'example.com')
    await wrapper.find('[data-testid="classic-domains-register"]').trigger('click')

    const button = wrapper.find('[data-testid="classic-domains-register"]')
    expect(button.attributes('disabled')).toBeDefined()
    expect(button.text()).toBe('domains.alreadyInCart')
  })

  it('offers a transfer for a taken domain', async () => {
    api.checkDomain.mockResolvedValue({ domain: 'taken.com', available: false, status: 'registered' })
    const wrapper = await mountDomains()

    await search(wrapper, 'taken.com')

    expect(wrapper.find('[data-testid="classic-domains-availability"]').text()).toContain('domains.taken')
    expect(wrapper.find('[data-testid="classic-domains-transfer"]').attributes('href')).toBe('/en/domains/transfer?domain=taken.com')
  })

  it('says so when the extension is not offered', async () => {
    api.checkDomain.mockResolvedValue({ domain: 'example.zzz', available: true, status: DomainAvailabilityStatus.Available })
    const wrapper = await mountDomains()

    await search(wrapper, 'example.zzz')

    expect(wrapper.find('[data-testid="classic-domains-unsupported"]').text()).toContain('domains.tldNotSupported')
    expect(wrapper.find('[data-testid="classic-domains-register"]').exists()).toBe(false)
  })

  it('shows the refusal sentence when the check fails', async () => {
    api.checkDomain.mockRejectedValue({ data: { detail: 'Registry unreachable.' } })
    const wrapper = await mountDomains()

    await search(wrapper, 'example.com')

    expect(wrapper.find('[data-testid="classic-domains-error"]').text()).toBe('Registry unreachable.')
  })

  it('searches at once when the route carries a domain', async () => {
    api.checkDomain.mockResolvedValue({ domain: 'prefilled.com', available: true, status: DomainAvailabilityStatus.Available })

    await mountDomains({ domain: 'prefilled.com' })

    expect(api.checkDomain).toHaveBeenCalledWith('prefilled.com')
  })

  it('lists every extension of the price table with the one-year prices', async () => {
    const wrapper = await mountDomains()

    const rows = wrapper.findAll('[data-testid="classic-domains-row"]')
    expect(rows).toHaveLength(2)
    expect(at(rows, 0).text()).toContain('.com')
    expect(at(rows, 0).text()).toContain('10.00')
    expect(at(rows, 0).text()).toContain('9.00')
    expect(at(rows, 0).text()).toContain('11.00')
  })

  it('filters the table to a category and back to all', async () => {
    const wrapper = await mountDomains()
    const categories = wrapper.findAll('[data-testid^="classic-domains-category-"]:not([data-testid="classic-domains-category-all"])')
    expect(categories).toHaveLength(2)

    await at(categories, 1).trigger('click')
    expect(wrapper.findAll('[data-testid="classic-domains-row"]').map((row) => row.text())).toEqual([expect.stringContaining('.dev')])

    await wrapper.find('[data-testid="classic-domains-category-all"]').trigger('click')
    expect(wrapper.findAll('[data-testid="classic-domains-row"]')).toHaveLength(2)
  })

  it('runs a quick search for a row, keeping the typed base name', async () => {
    api.checkDomain.mockResolvedValue({ domain: 'mysite.dev', available: true, status: DomainAvailabilityStatus.Available })
    const wrapper = await mountDomains()
    await wrapper.find('[data-testid="search-bar-input"]').setValue('mysite.com')

    await at(wrapper.findAll('[data-testid="classic-domains-quick-search"]'), 1).trigger('click')
    await flushPromises()

    expect(api.checkDomain).toHaveBeenCalledWith('mysite.dev')
  })

  it('renders the same extensions as cards for small screens', async () => {
    const wrapper = await mountDomains()

    expect(wrapper.findAll('[data-testid="classic-domains-card"]')).toHaveLength(2)
  })

  it('links the two promo boxes', async () => {
    const wrapper = await mountDomains()

    expect(wrapper.find('[data-testid="classic-domains-promo-hosting-link"]').attributes('href')).toBe('/en/hosting')
    expect(wrapper.find('[data-testid="classic-domains-promo-transfer-link"]').attributes('href')).toBe('/en/domains/transfer')
  })

  it('uses classic tokens only: no gradients, blur glows or aurora colour families', async () => {
    const wrapper = await mountDomains()
    const html = wrapper.find('[data-testid="classic-domains"]').html()

    expect(html).not.toMatch(/bg-gradient|blur-\[|animate-blob|shadow-halo|cyan-|violet-|secondary-/)
    expect(html).toContain('classic-')
  })

  it('draws the highlighted title word at the heading size in the brand colour', async () => {
    const wrapper = await mountDomains()

    const highlight = wrapper.find('[data-testid="classic-domains-title"] span')
    expect(highlight.text()).toContain('domains.titleHighlight')
    expect(highlight.classes()).toContain('text-classic-brand')
    expect(highlight.classes()).not.toContain('text-base')
  })

  it('keeps every section in the shared container and never caps the search', async () => {
    const wrapper = await mountDomains()

    expect(wrapper.findAll('.container-custom').length).toBeGreaterThanOrEqual(2)
    expect(wrapper.html()).not.toMatch(/max-w-(2xl|3xl|4xl|5xl|6xl)/)
  })
})
