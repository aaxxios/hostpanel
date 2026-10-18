/**
 * Unit tests for the product detail page: it finds the product named by the address, shows its plans
 * in the payer's currency, adds a plan to the cart in the chosen cycle, links the external order page
 * only when the deployment has one, and answers a missing product with a notice.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountPage, stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import ProductPage from '~/pages/products/[product].vue'
import CatalogProductDetail from '~/components/catalog/ProductDetail.vue'
import SectionsCTA from '~/components/sections/SectionsCTA.vue'
import MarketingSectionHeader from '~/components/marketing/MarketingSectionHeader.vue'
import { useCartStore } from '~/stores/cart'
import type { BillingCurrency } from '~/types/currency'
import type { PortalProduct } from '~/types/portalproduct'

/** The mocked catalogue endpoints, re-created for every test. */
const api = { fetchProducts: vi.fn() }

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

vi.mock('~/composables/apis/useCurrencyApi', () => ({
  /**
   * Stands in for the currency API composable.
   *
   * @returns Only the endpoint the page's store reaches for.
   */
  useCurrencyApi: () => ({ loadBillingCurrencies: () => Promise.resolve([usd]) }),
}))

/** A family with a parent and two plans. */
const catalogue: PortalProduct[] = [
  { id: 10, pid: 10, groupId: 3, name: 'SmartLearn System', slug: 'smartlearn-system', description: 'Learn online<br />✔ Courses' },
  { id: 11, pid: 11, groupId: 3, name: 'Starter', slug: 'starter', description: '✔ One site', pricing: { monthly: 10, annual: 100 } },
  { id: 12, pid: 12, groupId: 3, name: 'Pro', slug: 'pro', description: '✔ Two sites', pricing: { monthly: 20, annual: null } },
]

/** The spies of the latest mount, so a test reads the SEO tags the page published. */
let spies: ReturnType<typeof stubPublicPageRuntime>

/**
 * Mounts the page for one product address.
 *
 * @param product The `product` route parameter.
 * @returns The wrapper.
 */
const mountProduct = async (product = 'smartlearn-system') => {
  spies = stubPublicPageRuntime({ params: { product } })
  const wrapper = await mountPage(ProductPage, { components: { CatalogProductDetail, SectionsCTA, MarketingSectionHeader }, stubs: { 'i18n-t': true } })
  await flushPromises()
  return wrapper
}

describe('product detail page', () => {
  beforeEach(() => {
    api.fetchProducts.mockReset()
    api.fetchProducts.mockResolvedValue(catalogue)
    window.localStorage.clear()
    vi.stubGlobal('open', vi.fn())
    vi.stubGlobal('useRuntimeConfig', () => ({ public: { baseUrl: 'https://panel.test' } }))
  })

  it('builds its absolute SEO URLs from the configured site URL', async () => {
    await mountProduct()

    const tags = spies.useSeoMeta.mock.calls.at(-1)?.[0] as { ogImage: string, ogUrl: string }
    expect(tags.ogImage).toBe('https://panel.test/og-image.jpg')
    expect(tags.ogUrl).toMatch(/^https:\/\/panel\.test\//)
  })

  it('shows the product named by the address with its plans and their monthly prices', async () => {
    const wrapper = await mountProduct()

    expect(wrapper.find('[data-testid="product-name"]').text()).toBe('SmartLearn System')
    expect(wrapper.find('[data-testid="product-plan-11"] [data-testid="product-plan-price"]').text()).toContain('10.00')
    expect(wrapper.find('[data-testid="product-plan-12"] [data-testid="product-plan-price"]').text()).toContain('20.00')
  })

  it('offers the annual cycle only when a plan is priced for it, and switches the prices to it', async () => {
    const wrapper = await mountProduct()

    await wrapper.find('[data-testid="product-cycle-annual"]').trigger('click')

    expect(wrapper.find('[data-testid="product-plan-11"] [data-testid="product-plan-price"]').text()).toContain('100.00')
    expect(wrapper.find('[data-testid="product-plan-12"] [data-testid="product-plan-price"]').text()).toBe('—')
  })

  it('adds a plan to the cart with the selected cycle and the payer currency', async () => {
    const wrapper = await mountProduct()

    await wrapper.find('[data-testid="product-add-11"]').trigger('click')

    expect(useCartStore().items[0]).toMatchObject({ pid: 11, name: 'Starter', billingcycle: 'monthly', amount: 10, currency: 'USD' })
  })

  it('marks a plan that is already in the cart', async () => {
    const wrapper = await mountProduct()
    await wrapper.find('[data-testid="product-add-11"]').trigger('click')

    expect(wrapper.find('[data-testid="product-add-11"]').text()).toContain('hosting.inCart')
  })

  it('has no external trial button when the deployment runs no billing portal', async () => {
    const wrapper = await mountProduct()

    expect(wrapper.find('[data-testid="product-trial-11"]').exists()).toBe(false)
  })

  it('opens the billing-portal order page in a new tab when the deployment has one', async () => {
    vi.stubGlobal('useRuntimeConfig', () => ({ public: { billingPortalUrl: 'https://billing.test' } }))
    const wrapper = await mountProduct()

    await wrapper.find('[data-testid="product-trial-11"]').trigger('click')

    expect(window.open).toHaveBeenCalledWith('https://billing.test/cart.php?a=add&pid=11&billingcycle=monthly', '_blank')
  })

  it('answers an unknown product with a notice and the way back', async () => {
    const wrapper = await mountProduct('no-such-product')

    expect(wrapper.find('[data-testid="product-not-found"]').text()).toContain('products.notFound')
    expect(wrapper.find('[data-testid="product-back"]').attributes('href')).toBe('/en/products')
    expect(wrapper.find('[data-testid="product-detail"]').exists()).toBe(false)
  })
})
