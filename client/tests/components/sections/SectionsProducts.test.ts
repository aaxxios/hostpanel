/**
 * Unit tests for SectionsProducts: the static domains card first, then one card per catalogue
 * family, each painted in its own colour, with a demo or a learn-more call to action.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountPage, stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import SectionsProducts from '~/components/sections/SectionsProducts.vue'
import MarketingSectionHeader from '~/components/marketing/MarketingSectionHeader.vue'
import type { PortalProduct } from '~/types/portalproduct'
import type { PortalProductGroup } from '~/types/portalproductgroup'
import { familyColor } from '~/utils/catalogGroups'

/** The mocked catalogue endpoints, re-created for every test. */
const api = { fetchProducts: vi.fn(), fetchProductGroups: vi.fn() }

vi.mock('~/composables/apis/useCatalogApi', () => ({
  /**
   * Stands in for the catalogue API composable so the store runs without transport.
   *
   * @returns The mocked endpoints.
   */
  useCatalogApi: () => api,
}))

vi.mock('~/composables/apis/useCurrencyApi', () => ({
  /**
   * Stands in for the currency API composable.
   *
   * @returns Only the endpoint the page's store reaches for.
   */
  useCurrencyApi: () => ({ loadBillingCurrencies: () => Promise.resolve([]) }),
}))

/** One hosting plan and one SaaS family with its parent. */
const catalogue: PortalProduct[] = [
  { id: 1, pid: 1, groupId: 1, name: 'Web Hosting', slug: 'web-hosting', description: 'Fast hosting<br />✔ SSD<br />✔ SSL<br />✔ Backups<br />✔ Email' },
  { id: 10, pid: 10, groupId: 3, name: 'Alpha Suite', slug: 'alpha-suite', description: 'Learn online<br />✔ Courses' },
]

/** The groups: hosting and one family are shown; one is empty and one is switched off. */
const groups: PortalProductGroup[] = [
  { id: 3, name: 'Alpha Suite', isActive: true, productCount: 1 },
  { id: 1, name: 'Web Hosting', isActive: true, productCount: 1 },
  { id: 4, name: 'Beta Kit', isActive: true, productCount: 0 },
  { id: 5, name: 'Gamma Tool', isActive: false, productCount: 2 },
]

/** Mounts the section and lets the catalogue load. */
const mountSection = async () => {
  const wrapper = await mountPage(SectionsProducts, { components: { MarketingSectionHeader } })
  await flushPromises()
  return wrapper
}

describe('SectionsProducts', () => {
  beforeEach(() => {
    stubPublicPageRuntime()
    vi.stubGlobal('useRuntimeConfig', () => ({ public: { portalHostingDemoUrl: 'https://demo.test' } }))
    api.fetchProducts.mockReset()
    api.fetchProducts.mockResolvedValue(catalogue)
    api.fetchProductGroups.mockReset()
    api.fetchProductGroups.mockResolvedValue(groups)
    window.localStorage.clear()
  })

  it('starts with the domains card, which links to the domains page', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid^="home-product-"]').attributes('data-testid')).toBe('home-product-domains')
    expect(wrapper.find('[data-testid="home-product-link-domains"]').attributes('href')).toBe('/en/domains')
  })

  it('builds a card for every family the catalogue has, and none for the others', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="home-product-web-hosting"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="home-product-alpha-suite"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="home-product-beta-kit"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="home-product-gamma-tool"]').exists()).toBe(false)
  })

  it('puts hosting before the other families and asks only for the family groups products', async () => {
    const wrapper = await mountSection()

    const ids = wrapper.findAll('[data-testid^="home-product-"]').map((card) => card.attributes('data-testid') ?? '')
    const cards = ids.filter((id) => ['domains', 'web-hosting', 'alpha-suite'].includes(id.replace('home-product-', '')))
    expect(cards).toEqual(['home-product-domains', 'home-product-web-hosting', 'home-product-alpha-suite'])
    expect(api.fetchProducts).toHaveBeenCalledWith(expect.objectContaining({ gids: '1,3' }))
  })

  it('reads no products and shows only the domains card when the catalogue has no family', async () => {
    api.fetchProductGroups.mockResolvedValue([])
    const wrapper = await mountSection()

    expect(api.fetchProducts).not.toHaveBeenCalled()
    expect(wrapper.find('[data-testid="home-product-domains"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="home-product-web-hosting"]').exists()).toBe(false)
  })

  it('shows a family summary and at most three features', async () => {
    const wrapper = await mountSection()

    const hosting = wrapper.find('[data-testid="home-product-web-hosting"]')
    expect(hosting.text()).toContain('Fast hosting')
    expect(hosting.text()).toContain('Backups')
    expect(hosting.text()).not.toContain('Email')
  })

  it('paints each card in its own colour', async () => {
    const wrapper = await mountSection()

    const card = wrapper.find('[data-testid="home-product-alpha-suite"]').element.parentElement as HTMLElement
    expect(card.style.getPropertyValue('--accent')).toBe(familyColor('alpha-suite'))
  })

  it('uses the dark palette on the cards, which sit on a photograph', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="home-product-domains"]').classes()).toContain('dark')
  })

  it('opens a demo in a new tab when the family has one, and the learn-more page otherwise', async () => {
    const wrapper = await mountSection()

    const hosting = wrapper.find('[data-testid="home-product-cta-web-hosting"]')
    expect(hosting.attributes('href')).toBe('https://demo.test')
    expect(hosting.attributes('target')).toBe('_blank')
    expect(wrapper.find('[data-testid="home-product-cta-domains"]').attributes('href')).toBe('/en/domains')
  })

  it('links hosting to its learn-more page when no demo address is configured', async () => {
    vi.stubGlobal('useRuntimeConfig', () => ({ public: { portalHostingDemoUrl: '' } }))
    const wrapper = await mountSection()

    const hosting = wrapper.find('[data-testid="home-product-cta-web-hosting"]')
    expect(hosting.attributes('href')).toBe('/en/hosting')
    expect(hosting.attributes('target')).toBeUndefined()
  })

  it('links View all to the products page', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="home-products-all"]').attributes('href')).toBe('/en/products')
  })

  it('renders no image on a card that has none', async () => {
    const wrapper = await mountSection()

    const card = wrapper.find('[data-testid="home-product-domains"]')
    expect(card.exists()).toBe(true)
    expect(card.find('img').exists()).toBe(false)
    expect(card.find('nuxt-img-stub').exists()).toBe(false)
  })
})
