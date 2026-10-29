/**
 * Tests for the currencies store mutations: success refreshes the list, a refusal lands in
 * `actionError` with the API's own sentence and the action reports failure as a value.
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { useCurrenciesStore } from '@/stores/currencies'
import { json, problem, stubFetch } from '../helpers/fetch'
import { installStoreHooks } from '../helpers/storeHooks'

/**
 * A problem+json refusal carrying a finished sentence.
 *
 * @returns A 400 response with a `detail` sentence.
 */
const refusal = (): Response =>
  json({ type: 'CURRENCY_REFUSED', title: 'Refused', status: 400, detail: 'Not allowed.' }, 400)

describe('currenciesStore mutations', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('upsert returns true and refreshes the list', async () => {
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(json({}))
      .mockResolvedValueOnce(json([]))
    vi.stubGlobal('fetch', fetchMock)
    const store = useCurrenciesStore()

    expect(await store.upsert('EUR', {} as never)).toBe(true)
    expect(fetchMock).toHaveBeenCalledTimes(2)
    expect(store.actionError).toBeNull()
  })

  it('upsert returns false and stores the API sentence on refusal', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(refusal()))
    const store = useCurrenciesStore()

    expect(await store.upsert('EUR', {} as never)).toBe(false)
    expect(store.actionError).toBe('Not allowed.')
  })

  it('makeBase returns false with the fallback when nothing answered', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('offline')))
    const store = useCurrenciesStore()

    expect(await store.makeBase('EUR')).toBe(false)
    expect(store.actionError).toBe('Could not reach the server. Check your connection and try again.')
  })

  it('updateRates returns the result, null on refusal', async () => {
    const store = useCurrenciesStore()
    vi.stubGlobal('fetch', vi.fn()
      .mockResolvedValueOnce(json({ updated: ['EUR'], missing: [] }))
      .mockResolvedValueOnce(json([])))
    expect(await store.updateRates()).toEqual({ updated: ['EUR'], missing: [] })

    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(refusal()))
    expect(await store.updateRates()).toBeNull()
    expect(store.actionError).toBe('Not allowed.')
  })

  it('updateProductPrices returns the count, null on refusal', async () => {
    const store = useCurrenciesStore()
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(json(3)))
    expect(await store.updateProductPrices(['EUR'])).toBe(3)

    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(refusal()))
    expect(await store.updateProductPrices(['EUR'])).toBeNull()
    expect(store.actionError).toBe('Not allowed.')
  })
})

describe('currenciesStore.fetchAll', () => {
  installStoreHooks()

  it('fills the list and exposes the enabled ones', async () => {
    stubFetch(json([{ code: 'EUR', isEnabled: true }, { code: 'GBP', isEnabled: false }]))
    const store = useCurrenciesStore()

    await store.fetchAll()
    expect(store.enabledCurrencies.map(c => c.code)).toEqual(['EUR'])
  })

  it('keeps the API sentence on refusal', async () => {
    stubFetch(problem('Currencies locked.', 423))
    const store = useCurrenciesStore()

    await store.fetchAll()
    expect(store.error).toBe('Currencies locked.')
    expect(store.loading).toBe(false)
  })

  it('updateProductPrices returns the row count, or null with actionError', async () => {
    const store = useCurrenciesStore()

    stubFetch(json(12))
    expect(await store.updateProductPrices(['EUR'])).toBe(12)

    stubFetch(problem('No rates.', 409))
    expect(await store.updateProductPrices(['EUR'])).toBeNull()
    expect(store.actionError).toBe('No rates.')
  })
})
