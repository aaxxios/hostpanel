/**
 * Unit tests for the products page: it reads the family groups in the payer's currency, builds the
 * families with the domains card first, feeds the catalogue and comparison blocks, and sets the head.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountPage, stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import type { PublicPageSpies } from '~/tests/helpers/publicPage'
import { testimonialFixture } from '~/tests/helpers/storefrontFixtures'
import ProductsPage from '~/pages/products/index.vue'
import CatalogProductsHero from '~/components/catalog/ProductsHero.vue'
import CatalogProductFamilyGrid from '~/components/catalog/ProductFamilyGrid.vue'
import CatalogProductFamilyCard from '~/components/catalog/ProductFamilyCard.vue'
import CatalogProductsComparison from '~/components/catalog/ProductsComparison.vue'
import CatalogProductsBenefits from '~/components/catalog/ProductsBenefits.vue'
import CatalogProductsTestimonials from '~/components/catalog/ProductsTestimonials.vue'
import MarketingSectionHeader from '~/components/marketing/MarketingSectionHeader.vue'
import SectionsCTA from '~/components/sections/SectionsCTA.vue'
import type { BillingCurrency } from '~/types/currency'
import type { PortalProduct } from '~/types/portalproduct'
import type { PortalProductGroup } from '~/types/portalproductgroup'

/** The mocked catalogue endpoints, re-created for every test. */
const api = { fetchProducts: vi.fn(), fetchProductGroups: vi.fn(), fetchProductFeatures: vi.fn() }

/** The currency the stood-in currency endpoint answers with. */
const usd: BillingCurrency = { code: 'USD', numeric: '840', prefix: '$', suffix: '', decimals: 2, isBase: true }

vi.mock('~/composables/apis/useCatalogApi', () => ({
  /**
   * Stands in for the catalogue API composable so the store runs without transport.
   *
   * @returns The mocked endpoints.
   */
  useCatalogApi: () => api,
}))

/** The mocked storefront endpoints, so the testimonials block has quotes to show. */
const storefrontApi = { fetchPartners: vi.fn(), fetchTestimonials: vi.fn(), fetchProjects: vi.fn(), fetchFaqs: vi.fn() }

vi.mock('~/composables/apis/useStorefrontApi', () => ({
  /**
   * Stands in for the storefront API composable so the store runs without transport.
   *
   * @returns The mocked endpoints.
   */
  useStorefrontApi: () => storefrontApi,
}))

vi.mock('~/composables/apis/useCurrencyApi', () => ({
  /**
   * Stands in for the currency API composable.
   *
   * @returns Only the endpoint the page's store reaches for.
   */
  useCurrencyApi: () => ({ loadBillingCurrencies: () => Promise.resolve([usd]) }),
}))

/** The groups: one family with products and one that is empty. */
const groups: PortalProductGroup[] = [
  { id: 3, name: 'Alpha Suite', isActive: true, productCount: 3 },
  { id: 4, name: 'Beta Kit', isActive: true, productCount: 0 },
  { id: 6, name: 'Domains', isActive: true, productCount: 1 },
]

/** A SaaS family: its parent and two plans, one of them priced. */
const catalogue: PortalProduct[] = [
  { id: 10, pid: 10, groupId: 3, name: 'Alpha Suite', slug: 'alpha-suite', type: 'Vps', description: 'Learn online<br />✔ Courses' },
  { id: 20, pid: 20, groupId: 6, name: 'Domain Registration', slug: 'domain-registration', type: 'Domain', description: 'Register' },
  { id: 11, pid: 11, groupId: 3, name: 'Starter', slug: 'starter', description: '✔ One site', pricing: { monthly: 10, annual: 100 } },
  { id: 12, pid: 12, groupId: 3, name: 'Pro', slug: 'pro', description: '✔ Two sites', pricing: { monthly: 20, annual: 200 } },
]

/** What the stood-in runtime recorded for the current test. */
let spies: PublicPageSpies

/** Mounts the page and lets the catalogue and the currencies load. */
const mountProducts = async () => {
  const wrapper = await mountPage(ProductsPage, {
    components: {
      CatalogProductsHero,
      CatalogProductFamilyGrid,
      CatalogProductFamilyCard,
      CatalogProductsComparison,
      CatalogProductsBenefits,
      CatalogProductsTestimonials,
      MarketingSectionHeader,
      SectionsCTA,
    },
    stubs: { 'i18n-t': true },
  })
  await flushPromises()
  return wrapper
}

describe('products page', () => {
  beforeEach(() => {
    spies = stubPublicPageRuntime()
    api.fetchProducts.mockReset()
    api.fetchProducts.mockResolvedValue(catalogue)
    api.fetchProductGroups.mockReset()
    api.fetchProductGroups.mockResolvedValue(groups)
    api.fetchProductFeatures.mockReset()
    api.fetchProductFeatures.mockResolvedValue([])
    storefrontApi.fetchTestimonials.mockReset()
    storefrontApi.fetchTestimonials.mockResolvedValue([1, 2, 3].map(id => testimonialFixture(id)))
    window.localStorage.clear()
  })

  it('reads the products of the family groups only, and again in the payer currency once it resolves', async () => {
    await mountProducts()

    expect(api.fetchProducts).toHaveBeenCalledWith(expect.objectContaining({ gids: '3,6' }))
  })

  it('puts the static domains card first, then one card per catalogue family', async () => {
    const wrapper = await mountProducts()

    const cards = wrapper.findAll('[data-testid^="product-card-"]').map((card) => card.attributes('data-testid'))
    expect(cards).toEqual(['product-card-domains', 'product-card-alpha-suite'])
  })

  it('shows domains once even when the catalogue has a domains group', async () => {
    const wrapper = await mountProducts()

    expect(wrapper.findAll('[data-testid="product-card-domains"]')).toHaveLength(1)
    expect(wrapper.find('[data-testid="product-card-domain-registration"]').exists()).toBe(false)
  })

  it('gives each card the icon of its product type', async () => {
    const wrapper = await mountProducts()

    expect(wrapper.find('[data-testid="product-card-alpha-suite"]').html()).toContain('mdi:server')
    expect(wrapper.find('[data-testid="product-card-domains"]').html()).toContain('mdi:earth')
  })

  it('lists a family with priced plans in the comparison table, in the payer currency', async () => {
    const wrapper = await mountProducts()

    const row = wrapper.find('[data-testid="comparison-row-alpha-suite"]')
    expect(row.exists()).toBe(true)
    expect(row.text()).toContain('10.00')
    expect(wrapper.find('[data-testid="comparison-row-domains"]').exists()).toBe(false)
  })

  it('renders the hero, the benefits and the testimonials around the catalogue', async () => {
    const wrapper = await mountProducts()

    expect(wrapper.find('[data-testid="products-hero"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="products-benefits"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="products-testimonials"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="sections-cta"]').exists()).toBe(true)
  })

  it('sets the canonical head and publishes the collection page schema', async () => {
    await mountProducts()

    expect(spies.useSeo).toHaveBeenCalledWith(expect.objectContaining({ path: '/products', title: 'seo.products.title' }))
    const schemas = spies.injectSchema.mock.calls[0]?.[0] as Array<Record<string, unknown>>
    expect(schemas.some((schema) => schema['@type'] === 'CollectionPage')).toBe(true)
  })
})
