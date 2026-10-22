/**
 * Unit tests for SectionsProducts: the static domains card first, then one card per catalogue
 * family, each painted in its own colour, with a demo or a learn-more call to action.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountPage, stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import SectionsProducts from '~/components/sections/SectionsProducts.vue'
import ProductFamilyCard from '~/components/catalog/ProductFamilyCard.vue'
import { CLASSIC_COMPONENTS } from '~/tests/helpers/classicKit'
import type { PortalProduct } from '~/types/portalproduct'
import type { PortalProductGroup } from '~/types/portalproductgroup'
import { HOME_SECTION_SCROLL_MARGIN_CLASS } from '~/constants/homeSections'
import { HomeSectionId } from '~/types/homesectionid'

/** The mocked catalogue endpoints, re-created for every test. */
const api = { fetchProducts: vi.fn(), fetchProductGroups: vi.fn(), fetchProductFeatures: vi.fn() }

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
  { id: 1, pid: 1, groupId: 1, name: 'Web Hosting', slug: 'web-hosting', type: 'SharedHosting', description: 'Fast hosting<br />✔ SSD<br />✔ SSL<br />✔ Backups<br />✔ Email' },
  { id: 10, pid: 10, groupId: 3, name: 'Alpha Suite', slug: 'alpha-suite', type: 'Vps', description: 'Learn online<br />✔ Courses' },
  { id: 20, pid: 20, groupId: 6, name: 'Domain Registration', slug: 'domain-registration', type: 'Domain', description: 'Register domains' },
  { id: 30, pid: 30, groupId: 7, name: 'Plain Plan', slug: 'plain-plan', type: 'Other', description: 'No bullets here' },
]

/** The groups: hosting and one family are shown; one is empty and one is switched off. */
const groups: PortalProductGroup[] = [
  { id: 3, name: 'Alpha Suite', isActive: true, productCount: 1 },
  { id: 1, name: 'Web Hosting', isActive: true, productCount: 1 },
  { id: 6, name: 'Domains', isActive: true, productCount: 1 },
  { id: 7, name: 'Plain Plan', isActive: true, productCount: 1 },
  { id: 4, name: 'Beta Kit', isActive: true, productCount: 0 },
  { id: 5, name: 'Gamma Tool', isActive: false, productCount: 2 },
]

/** Mounts the section and lets the catalogue load. */
const mountSection = async () => {
  const wrapper = await mountPage(SectionsProducts, { components: { ...CLASSIC_COMPONENTS, CatalogProductFamilyCard: ProductFamilyCard } })
  await flushPromises()
  return wrapper
}

describe('SectionsProducts', () => {
  beforeEach(() => {
    stubPublicPageRuntime()
    api.fetchProducts.mockReset()
    api.fetchProducts.mockResolvedValue(catalogue)
    api.fetchProductGroups.mockReset()
    api.fetchProductGroups.mockResolvedValue(groups)
    api.fetchProductFeatures.mockReset()
    api.fetchProductFeatures.mockResolvedValue([])
    window.localStorage.clear()
  })

  it('carries the services anchor the header links to, with room above it', async () => {
    const section = (await mountSection()).find('[data-testid="sections-products"]')

    expect(section.attributes('id')).toBe(HomeSectionId.Services)
    expect(section.classes()).toContain(HOME_SECTION_SCROLL_MARGIN_CLASS)
  })

  it('starts with the domains card, which links to the domains page', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid^="product-card-"]').attributes('data-testid')).toBe('product-card-domains')
    expect(wrapper.find('[data-testid="product-link-domains"]').attributes('href')).toBe('/en/domains')
  })

  it('builds a card for every family the catalogue has, and none for the others', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="product-card-web-hosting"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="product-card-alpha-suite"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="product-card-beta-kit"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="product-card-gamma-tool"]').exists()).toBe(false)
  })

  it('puts hosting before the other families and asks only for the family groups products', async () => {
    api.fetchProducts.mockResolvedValue(catalogue.filter((item) => item.groupId !== 6 && item.groupId !== 7))
    api.fetchProductGroups.mockResolvedValue(groups.filter((item) => item.id !== 6 && item.id !== 7))
    const wrapper = await mountSection()

    const ids = wrapper.findAll('[data-testid^="product-card-"]').map((card) => card.attributes('data-testid') ?? '')
    const cards = ids.filter((id) => ['domains', 'web-hosting', 'alpha-suite'].includes(id.replace('product-card-', '')))
    expect(cards).toEqual(['product-card-domains', 'product-card-web-hosting', 'product-card-alpha-suite'])
    expect(api.fetchProducts).toHaveBeenCalledWith(expect.objectContaining({ gids: '1,3' }))
  })

  it('reads no products and shows only the domains card when the catalogue has no family', async () => {
    api.fetchProductGroups.mockResolvedValue([])
    const wrapper = await mountSection()

    expect(api.fetchProducts).not.toHaveBeenCalled()
    expect(wrapper.find('[data-testid="product-card-domains"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="product-card-web-hosting"]').exists()).toBe(false)
  })

  it('shows a family summary and at most three features', async () => {
    const wrapper = await mountSection()

    const hosting = wrapper.find('[data-testid="product-card-web-hosting"]')
    expect(hosting.text()).toContain('Fast hosting')
    expect(hosting.text()).toContain('Backups')
    expect(hosting.text()).not.toContain('Email')
  })

  it('shows domains once: the domains group is covered by the static card', async () => {
    const wrapper = await mountSection()

    expect(wrapper.findAll('[data-testid="product-card-domains"]')).toHaveLength(1)
    expect(wrapper.find('[data-testid="product-card-domain-registration"]').exists()).toBe(false)
  })

  it('draws each family tile from the type of its products with a full icon name', async () => {
    const wrapper = await mountSection()

    expect(wrapper.html()).not.toContain('mdi:mdi:')
    expect(wrapper.find('[data-testid="product-card-web-hosting"]').html()).toContain('mdi:cloud-check')
    expect(wrapper.find('[data-testid="product-card-alpha-suite"]').html()).toContain('mdi:server')
  })

  it('lists no features, and no placeholder, for a family with none', async () => {
    const wrapper = await mountSection()

    const plain = wrapper.find('[data-testid="product-card-plain-plan"]')
    expect(plain.text()).not.toContain('SaaS')
    expect(plain.findAll('svg')).toHaveLength(0)
  })

  it('uses the specification lines when a description lists no features', async () => {
    api.fetchProductFeatures.mockImplementation((groupId: number) =>
      Promise.resolve(groupId === 7 ? [{ id: 1, productId: 30, label: 'Disk', value: '10 GB', sortOrder: 1 }] : []))
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="product-card-plain-plan"]').text()).toContain('Disk: 10 GB')
  })

  it('links View all to the products page', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="home-products-all"]').attributes('href')).toBe('/en/products')
  })

  it('draws each card with the shared family card: a generated cover and the bullets always visible', async () => {
    const wrapper = await mountSection()

    const hosting = wrapper.find('[data-testid="product-card-web-hosting"]')
    expect(hosting.find('[data-testid="product-cover"]').exists()).toBe(true)
    expect(hosting.find('[data-testid="product-bullets"]').exists()).toBe(true)
    expect(hosting.html()).not.toContain('opacity-0 group-hover:opacity-100')
  })

  it('shows the Featured badge only on a family whose plan is recommended', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="product-card-domains"] [data-testid="product-featured"]').exists()).toBe(false)
  })
})
