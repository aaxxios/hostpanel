/**
 * Endpoints behind the currencies store: the configured-currency list, per-currency upsert,
 * base switching, exchange-rate refresh and the bulk product-price recompute.
 *
 * No state lives here; every function rejects with an `ApiError` carrying the problem+json body.
 */

import { useApi } from '@/composables/useApi'
import type { CurrencyDto, UpdateExchangeRatesResult, UpsertCurrencyPayload } from '@/types/currency'

/**
 * Endpoint functions for `/admin/currencies`.
 *
 * @returns One typed function per endpoint.
 */
export const useCurrenciesApi = () => {
  const { request } = useApi()

  /**
   * Lists the configured currencies, base first.
   *
   * @returns Every configured currency.
   */
  const listCurrencies = (): Promise<CurrencyDto[]> => request<CurrencyDto[]>('/admin/currencies')

  /**
   * Creates the currency under the code, or rewrites the one already stored there.
   *
   * @param code - ISO 4217 alpha code, any case.
   * @param payload - Numeric code, display, rate and switch.
   * @returns Nothing; the API answers 204.
   */
  const upsertCurrency = (code: string, payload: UpsertCurrencyPayload): Promise<void> =>
    request<void>(`/admin/currencies/${code}`, { method: 'PUT', body: JSON.stringify(payload) })

  /**
   * Makes the currency the base.
   *
   * @param code - ISO 4217 alpha code, any case.
   * @returns Nothing; the API answers 204.
   */
  const makeBaseCurrency = (code: string): Promise<void> =>
    request<void>(`/admin/currencies/${code}/make-base`, { method: 'POST' })

  /**
   * Refreshes every non-base rate from the exchange-rate source.
   *
   * @returns Which codes were updated and which the source did not quote.
   */
  const updateExchangeRates = (): Promise<UpdateExchangeRatesResult> =>
    request<UpdateExchangeRatesResult>('/admin/currencies/update-rates', { method: 'POST' })

  /**
   * Recomputes every product's prices in the given currencies from their base prices.
   *
   * @param codes - Non-base currency codes to recompute.
   * @returns How many price rows were written.
   */
  const updateProductPrices = (codes: string[]): Promise<number> =>
    request<number>('/admin/currencies/update-product-prices', {
      method: 'POST',
      body: JSON.stringify({ currencies: codes }),
    })

  return { listCurrencies, upsertCurrency, makeBaseCurrency, updateExchangeRates, updateProductPrices }
}
