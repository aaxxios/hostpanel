/**
 * Tests for `stores/catalog.ts`.
 *
 * The store, not the public pages, owns the product lists, the loading flag and the failure
 * sentence, and it always asks for prices in the payer's own currency, so these assertions pin
 * the contract the pages render from.
 *
 * @module tests/stores/catalog.test
 */

import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

/** The mocked catalogue endpoints, re-created for every test. */
const api = {
  fetchProducts: vi.fn(),
  fetchProductFeatures: vi.fn(),
  fetchSlides: vi.fn(),
  fetchCountries: vi.fn(),
  fetchTldPricing: vi.fn(),
  fetchCurrencies: vi.fn(),
  checkDomain: vi.fn()
}

/** The payer the currency store stand-in reports; tests change it to simulate a currency switch. */
const payer = { code: 'USD' as string | null }

vi.mock('~/composables/apis/useCatalogApi', () => ({
  /**
   * Stands in for the real API composable so the store runs without transport.
   *
   * @returns The mocked endpoints.
   */
  useCatalogApi: () => api
}))

vi.mock('~/stores/currency', () => ({
  /**
   * Stands in for the currency store: a payer code and the money shape for formatting.
   *
   * @returns The two members the catalogue store reads.
   */
  useCurrencyStore: () => ({
    get payerCode() {
      return payer.code
    },
    moneyCurrencyFor: () => ({ code: 'USD', prefix: '$', suffix: '', decimals: 2 })
  })
}))

const { useCatalogStore } = await import('~/stores/catalog')
const { formatMoney } = await import('~/utils/formatMoney')

/** An API refusal as `apiErrorMessage` reads it. */
const refusal = { data: { detail: 'Catalogue unavailable.' } }

/** The money shape the currency stand-in returns. */
const usd = { code: 'USD', prefix: '$', suffix: '', decimals: 2 }

describe('useCatalogStore.loadProducts', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    payer.code = 'USD'
    Object.values(api).forEach(fn => fn.mockReset())
  })

  it('asks for the filter in the payer currency and keeps the list', async () => {
    api.fetchProducts.mockResolvedValue([{ id: 1, name: 'Starter' }])
    const store = useCatalogStore()

    const list = await store.loadProducts({ gid: 1 })

    expect(api.fetchProducts).toHaveBeenCalledWith({ gid: 1, currency: 'USD' })
    expect(list).toEqual([{ id: 1, name: 'Starter' }])
    expect(store.productsFor({ gid: 1 })).toEqual(list)
    expect(store.loading).toBe(false)
    expect(store.error).toBe('')
  })

  it('omits the currency while the payer currency is not known yet', async () => {
    payer.code = null
    api.fetchProducts.mockResolvedValue([])
    const store = useCatalogStore()

    await store.loadProducts({ gids: '1,3' })

    expect(api.fetchProducts).toHaveBeenCalledWith({ gids: '1,3', currency: undefined })
  })

  it('keeps one list per filter and per currency', async () => {
    api.fetchProducts.mockResolvedValueOnce([{ id: 1 }]).mockResolvedValueOnce([{ id: 2 }])
    const store = useCatalogStore()

    await store.loadProducts({ gid: 1 })
    payer.code = 'AMD'
    await store.loadProducts({ gid: 1 })

    expect(store.productsFor({ gid: 1 })).toEqual([{ id: 2 }])
    payer.code = 'USD'
    expect(store.productsFor({ gid: 1 })).toEqual([{ id: 1 }])
  })

  it('keeps the API sentence and an empty list when the read is refused', async () => {
    api.fetchProducts.mockRejectedValue(refusal)
    const store = useCatalogStore()

    const list = await store.loadProducts({ gid: 1 })

    expect(list).toEqual([])
    expect(store.error).toBe('Catalogue unavailable.')
    expect(store.loading).toBe(false)
  })

  it('clears the previous sentence when the next read succeeds', async () => {
    api.fetchProducts.mockRejectedValueOnce(refusal).mockResolvedValueOnce([])
    const store = useCatalogStore()

    await store.loadProducts()
    await store.loadProducts()

    expect(store.error).toBe('')
  })

  it('finds the domain product by name in the whole catalogue', async () => {
    api.fetchProducts.mockResolvedValue([{ id: 4, name: 'Hosting' }, { id: 9, name: 'Domain Registration' }])
    const store = useCatalogStore()
    expect(store.domainProductId).toBeNull()

    await store.loadProducts()

    expect(store.domainProductId).toBe(9)
  })
})

describe('useCatalogStore.planCards', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    payer.code = 'USD'
    Object.values(api).forEach(fn => fn.mockReset())
  })

  it('maps hosting plans to priced cards with the page-supplied link', async () => {
    api.fetchProducts.mockResolvedValue([
      { id: 5, name: 'Pro', description: 'Fast hosting', pricing: { monthly: 12, annual: 120 } },
      { id: 6, name: 'Free', description: '', pricing: { monthly: 0, annual: 0 } },
      { id: 7, name: 'Quote', description: '', pricing: null }
    ])
    const store = useCatalogStore()
    await store.loadHostingPlans()

    const cards = store.planCards(id => `/en/configure/${id}`)

    expect(cards[0]).toMatchObject({
      id: 5,
      name: 'Pro',
      description: 'Fast hosting',
      isFree: false,
      priceMonthly: formatMoney(12, usd),
      priceAnnual: formatMoney(10, usd),
      href: '/en/configure/5'
    })
    expect(cards[1]?.isFree).toBe(true)
    expect(cards[2]).toMatchObject({ isFree: false, priceMonthly: '—', priceAnnual: '—' })
  })

  it('has no cards before the plans were read', () => {
    expect(useCatalogStore().planCards(id => `/${id}`)).toEqual([])
  })
})

describe('useCatalogStore reference data', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    payer.code = 'USD'
    Object.values(api).forEach(fn => fn.mockReset())
  })

  it('keeps specification lines per product group', async () => {
    api.fetchProductFeatures.mockResolvedValue([{ id: 1, productId: 5, label: 'Disk', value: '10 GB', sortOrder: 0 }])
    const store = useCatalogStore()

    await store.loadProductFeatures(1)

    expect(api.fetchProductFeatures).toHaveBeenCalledWith(1)
    expect(store.featuresFor(1)).toHaveLength(1)
    expect(store.featuresFor(2)).toEqual([])
  })

  it('reads the slides in the given language', async () => {
    api.fetchSlides.mockResolvedValue([{ id: 1, title: 'Hello' }])
    const store = useCatalogStore()

    await store.loadSlides('hy')

    expect(api.fetchSlides).toHaveBeenCalledWith('hy')
    expect(store.slides).toEqual([{ id: 1, title: 'Hello' }])
  })

  it('reads the countries and empties the list when the read is refused', async () => {
    api.fetchCountries.mockResolvedValueOnce([{ value: 'AM', label: 'Armenia' }]).mockRejectedValueOnce(refusal)
    const store = useCatalogStore()

    await store.loadCountries()
    expect(store.countries).toEqual([{ value: 'AM', label: 'Armenia' }])

    await store.loadCountries()
    expect(store.countries).toEqual([])
    expect(store.error).toBe('Catalogue unavailable.')
  })

  it('reads the TLD price table in the payer currency and nulls it on a refusal', async () => {
    api.fetchTldPricing.mockResolvedValueOnce({ currency: { code: 'USD', prefix: '$' }, pricing: {} }).mockRejectedValueOnce(refusal)
    const store = useCatalogStore()

    await store.loadTldPricing()
    expect(api.fetchTldPricing).toHaveBeenCalledWith('USD')
    expect(store.tldPricing).toMatchObject({ currency: { code: 'USD' } })

    await store.loadTldPricing()
    expect(store.tldPricing).toBeNull()
  })
})
