/**
 * Pinia store for admin currency configuration.
 *
 * Provides the configured currency list, per-currency upsert, base switching,
 * exchange-rate refresh, and the bulk product-price recompute. The only caller of
 * `useCurrenciesApi`; a refusal is kept as the API's own sentence, never thrown.
 */

import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { useCurrenciesApi } from '@/composables/apis/useCurrenciesApi'
import { runAction } from '@/lib/runAction'
import type { CurrencyDto, UpdateExchangeRatesResult, UpsertCurrencyPayload } from '@/types/currency'

/** Manages the configured-currency list and admin currency operations. */
export const useCurrenciesStore = defineStore('currencies', () => {
  const api = useCurrenciesApi()

  /** All configured currencies, base first. */
  const currencies = ref<CurrencyDto[]>([])

  /** Whether a fetch is in progress. */
  const loading = ref(false)

  /** Sentence for the last failed load, null when none. */
  const error = ref<string | null>(null)

  /** Sentence for the last failed mutation (save, make base, rates, prices), null when none. */
  const actionError = ref<string | null>(null)

  /** Currencies a payer may currently be billed in. */
  const enabledCurrencies = computed<CurrencyDto[]>(() => currencies.value.filter(c => c.isEnabled))

  /**
   * Fetches all configured currencies from the backend.
   *
   * @returns Resolves when the request is over; a refusal is left in `error`.
   */
  const fetchAll = async (): Promise<void> => {
    loading.value = true
    const result = await runAction(error, api.listCurrencies)
    loading.value = false
    if (result.ok) {
      currencies.value = result.value
    }
  }

  /**
   * Creates a currency under the code, or rewrites the one already stored there, then refreshes.
   *
   * @param code - ISO 4217 alpha code, any case.
   * @param payload - The currency's numeric code, display, rate and switch.
   * @returns True when saved; false when refused (see `actionError`).
   */
  const upsert = async (code: string, payload: UpsertCurrencyPayload): Promise<boolean> => {
    const result = await runAction(actionError, () => api.upsertCurrency(code, payload))
    if (result.ok) {
      await fetchAll()
    }
    return result.ok
  }

  /**
   * Makes the given currency the base and refreshes the list.
   *
   * @param code - ISO 4217 alpha code, any case.
   * @returns True when switched; false when refused (see `actionError`).
   */
  const makeBase = async (code: string): Promise<boolean> => {
    const result = await runAction(actionError, () => api.makeBaseCurrency(code))
    if (result.ok) {
      await fetchAll()
    }
    return result.ok
  }

  /**
   * Refreshes every non-base rate from the exchange-rate source and refreshes the list.
   *
   * @returns Which codes were updated and which the source did not quote; null when refused (see `actionError`).
   */
  const updateRates = async (): Promise<UpdateExchangeRatesResult | null> => {
    const result = await runAction(actionError, api.updateExchangeRates)
    if (!result.ok) {
      return null
    }
    await fetchAll()
    return result.value
  }

  /**
   * Recomputes every product's prices in the given currencies from their base prices.
   *
   * @param codes - Non-base currency codes to recompute.
   * @returns How many price rows were written; null when refused (see `actionError`).
   */
  const updateProductPrices = async (codes: string[]): Promise<number | null> => {
    const result = await runAction(actionError, () => api.updateProductPrices(codes))
    return result.ok ? result.value : null
  }

  return {
    currencies, loading, error, actionError, enabledCurrencies,
    fetchAll, upsert, makeBase, updateRates, updateProductPrices,
  }
})
