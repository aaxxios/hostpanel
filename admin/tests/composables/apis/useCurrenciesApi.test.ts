/**
 * Tests for the currencies API composable: paths, verbs and bodies through the real `useApi`
 * transport with `fetch` stubbed.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useCurrenciesApi } from '@/composables/apis/useCurrenciesApi'
import { json, problem, stubFetch, callOf } from '../../helpers/fetch'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('useCurrenciesApi', () => {
  it('listCurrencies reads /admin/currencies', async () => {
    const mock = stubFetch(json([{ code: 'EUR' }]))

    expect(await useCurrenciesApi().listCurrencies()).toEqual([{ code: 'EUR' }])
    expect(callOf(mock)).toMatchObject({ url: '/api/admin/currencies', method: 'GET' })
  })

  it('upsertCurrency PUTs the payload under the code', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))

    await useCurrenciesApi().upsertCurrency('EUR', { rate: 1.1 } as never)
    expect(callOf(mock)).toMatchObject({ url: '/api/admin/currencies/EUR', method: 'PUT', body: { rate: 1.1 } })
  })

  it('makeBaseCurrency POSTs to make-base', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))

    await useCurrenciesApi().makeBaseCurrency('USD')
    expect(callOf(mock)).toMatchObject({ url: '/api/admin/currencies/USD/make-base', method: 'POST' })
  })

  it('updateExchangeRates POSTs to update-rates and returns the result', async () => {
    const mock = stubFetch(json({ updated: ['EUR'], missing: [] }))

    expect(await useCurrenciesApi().updateExchangeRates()).toEqual({ updated: ['EUR'], missing: [] })
    expect(callOf(mock)).toMatchObject({ url: '/api/admin/currencies/update-rates', method: 'POST' })
  })

  it('updateProductPrices sends the codes and returns the row count', async () => {
    const mock = stubFetch(json(12))

    expect(await useCurrenciesApi().updateProductPrices(['EUR', 'GBP'])).toBe(12)
    expect(callOf(mock)).toMatchObject({
      url: '/api/admin/currencies/update-product-prices',
      method: 'POST',
      body: { currencies: ['EUR', 'GBP'] },
    })
  })

  it('rejects when the API refuses', async () => {
    stubFetch(problem('Not allowed.'))

    await expect(useCurrenciesApi().makeBaseCurrency('USD')).rejects.toBeTruthy()
  })
})
