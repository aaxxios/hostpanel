/**
 * Unit tests for the public domains page: it hands the template's component the price table, the
 * lookup state and the cart, adds a registration to the cart only when it can be sold in the
 * payer's currency, and sets the head and the structured data.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { defineComponent, h, ref } from 'vue'
import { mountPage, stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import type { PublicPageSpies } from '~/tests/helpers/publicPage'
import DomainsPage from '~/pages/domains/index.vue'
import { useCartStore } from '~/stores/cart'
import { useDomainsStore } from '~/stores/domains'
import { ALL_CATEGORY } from '~/templates/aurora/types/allcategory'
import type { BillingCurrency } from '~/types/currency'
import type { TldPricing } from '~/types/tldpricing'
import { CartItemType } from '~/types/cartitemtype'
import { DomainOrderAction } from '~/types/domainorderaction'
import { DOMAIN_SEARCH_QUERY_PARAM } from '~/constants/domainSearchQuery'
import { TemplateName } from '~/types/templatename'

/** The mocked catalogue endpoints, re-created for every test. */
const catalogApi = { fetchProducts: vi.fn(), fetchTldPricing: vi.fn(), checkDomain: vi.fn() }

/** The currency the stood-in currency endpoint answers with. */
const usd: BillingCurrency = { code: 'USD', numeric: '840', prefix: '$', suffix: '', decimals: 2, isBase: true }

vi.mock('~/composables/apis/useCatalogApi', () => ({
  /**
   * Stands in for the catalogue API composable so the store runs without transport.
   *
   * @returns The mocked endpoints.
   */
  useCatalogApi: () => catalogApi,
}))
vi.mock('~/composables/apis/useCurrencyApi', () => ({
  /**
   * Stands in for the currency API composable.
   *
   * @returns Only the endpoint the page's store reaches for.
   */
  useCurrencyApi: () => ({ loadBillingCurrencies: () => Promise.resolve([usd]) }),
}))
vi.mock('~/composables/apis/useDomainsApi', () => ({
  /**
   * Stands in for the domains API composable, which the page never reaches.
   *
   * @returns No endpoints.
   */
  useDomainsApi: () => ({}),
}))
vi.mock('~/composables/apis/useBillingApi', () => ({
  /**
   * Stands in for the billing API composable, which the page never reaches.
   *
   * @returns No endpoints.
   */
  useBillingApi: () => ({}),
}))
vi.mock('~/composables/apis/useClientApi', () => ({
  /**
   * Stands in for the client API composable, which the page never reaches.
   *
   * @returns No endpoints.
   */
  useClientApi: () => ({}),
}))

/**
 * Builds a TLD price table with one extension.
 *
 * @param sellCurrency The currency the extension sells in.
 * @returns The table.
 */
const pricing = (sellCurrency: string): TldPricing => ({
  currency: { code: 'USD', prefix: '$' },
  pricing: { '.com': { sellCurrency, register: { 1: '10.00' }, categories: ['popular'] } },
})

/** What the template component is given, recorded for the assertions. */
const received = { props: {} as Record<string, unknown> }

/** Stand-in for the template's domains component: records its props. */
const TemplateDomains = defineComponent({
  name: 'TemplateDomains',
  props: {
    priceRows: { type: Array, default: () => [] },
    results: { type: Array, default: () => [] },
    pending: { type: Boolean, default: false },
    categories: { type: Array, default: () => [] },
    activeCategory: { type: String, default: '' },
    inCart: { type: Array, default: () => [] },
  },
  emits: ['search', 'update:category', 'add'],
  setup(props) {
    received.props = props
    return () => h('div', { 'data-testid': 'template-domains' })
  },
})

/** What the stood-in runtime recorded for the current test. */
let spies: PublicPageSpies

/**
 * Mounts the page and lets the price table, the catalogue and the currencies load.
 *
 * @returns The wrapper.
 */
const mountDomains = async (template: TemplateName = TemplateName.Aurora) => {
  vi.stubGlobal('useTemplate', () => ({ slot: () => TemplateDomains, name: ref(template) }))
  const wrapper = await mountPage(DomainsPage)
  await flushPromises()
  return wrapper
}

describe('domains page', () => {
  beforeEach(() => {
    spies = stubPublicPageRuntime()
    vi.stubGlobal('useTemplate', () => ({ slot: () => TemplateDomains }))
    catalogApi.fetchProducts.mockReset()
    catalogApi.fetchProducts.mockResolvedValue([{ id: 7, pid: 7, groupId: 2, name: 'Domain Registration' }])
    catalogApi.fetchTldPricing.mockReset()
    catalogApi.fetchTldPricing.mockResolvedValue(pricing('USD'))
    window.localStorage.clear()
  })

  it('hands the template the price rows, the categories and the all-categories filter', async () => {
    await mountDomains()

    expect((received.props.priceRows as Array<{ tld: string }>).map((row) => row.tld)).toEqual(['.com'])
    expect(received.props.categories).toEqual(expect.arrayContaining([ALL_CATEGORY, 'popular']))
    expect(received.props.activeCategory).toBe(ALL_CATEGORY)
  })

  it('follows the category the template reports', async () => {
    const wrapper = await mountDomains()

    wrapper.findComponent(TemplateDomains).vm.$emit('update:category', 'popular')
    await flushPromises()

    expect(received.props.activeCategory).toBe('popular')
  })

  it('adds a registration to the cart in the payer currency, once', async () => {
    const wrapper = await mountDomains()

    wrapper.findComponent(TemplateDomains).vm.$emit('add', 'example.com')
    wrapper.findComponent(TemplateDomains).vm.$emit('add', 'example.com')
    await flushPromises()

    const items = useCartStore().items
    expect(items).toHaveLength(1)
    expect(items[0]).toMatchObject({
      pid: 7, name: 'example.com', domain: 'example.com', itemType: CartItemType.Domain, domainAction: DomainOrderAction.Register, tld: 'com', years: 1, amount: 10, currency: 'USD',
    })
    expect(received.props.inCart).toEqual(['example.com'])
  })

  it('does not add an extension that is not in the price table', async () => {
    const wrapper = await mountDomains()

    wrapper.findComponent(TemplateDomains).vm.$emit('add', 'example.org')
    await flushPromises()

    expect(useCartStore().items).toEqual([])
  })

  it('adds an extension that sells in another currency, at the payer-currency amount the API converted', async () => {
    catalogApi.fetchTldPricing.mockResolvedValue(pricing('AMD'))
    const wrapper = await mountDomains()
    useDomainsStore().lookupResults = [{ name: 'example.com', status: 'available' }] as never
    await flushPromises()

    wrapper.findComponent(TemplateDomains).vm.$emit('add', 'example.com')
    await flushPromises()

    const items = useCartStore().items
    expect(items).toHaveLength(1)
    expect(items[0]).toMatchObject({ domain: 'example.com', amount: 10, currency: 'USD' })
  })

  it('adds nothing when the operator has no domain product', async () => {
    catalogApi.fetchProducts.mockResolvedValue([])
    const wrapper = await mountDomains()

    wrapper.findComponent(TemplateDomains).vm.$emit('add', 'example.com')
    await flushPromises()

    expect(useCartStore().items).toEqual([])
  })

  it('looks the typed name up at once when the address carries it', async () => {
    spies = stubPublicPageRuntime({ query: { [DOMAIN_SEARCH_QUERY_PARAM]: 'example.com' } })
    catalogApi.checkDomain.mockReset()
    catalogApi.checkDomain.mockResolvedValue({ available: true })

    await mountDomains()

    expect(catalogApi.checkDomain).toHaveBeenCalledWith('example.com')
  })

  it('looks nothing up without a name, and leaves the search to the classic template', async () => {
    catalogApi.checkDomain.mockReset()
    await mountDomains()
    expect(catalogApi.checkDomain).not.toHaveBeenCalled()

    spies = stubPublicPageRuntime({ query: { [DOMAIN_SEARCH_QUERY_PARAM]: 'example.com' } })
    await mountDomains(TemplateName.Classic)
    expect(catalogApi.checkDomain).not.toHaveBeenCalled()
  })

  it('sets the canonical head and publishes the collection page schema', async () => {
    await mountDomains()

    expect(spies.useSeo).toHaveBeenCalledWith(expect.objectContaining({ path: '/domains' }))
    const schemas = spies.injectSchema.mock.calls[0]?.[0] as Array<Record<string, unknown>>
    expect(schemas.some((schema) => schema['@type'] === 'CollectionPage')).toBe(true)
  })
})
