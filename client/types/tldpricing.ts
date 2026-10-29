/**
 * The TLD price table, as `GET /api/portal/public/tld-pricing` returns it.
 *
 * Each action's prices are a list of period entries, shortest period first; read one with
 * `priceForYears` from `utils/tldPeriodPrice`.
 */

import type { TldPriceEntry } from '~/types/tldpriceentry'

/** The TLD price table in the payer's currency. */
export interface TldPricing {
  /** Currency every price below is quoted in. */
  currency: {
    /** ISO 4217 code, e.g. "AMD". */
    code: string
    /** Symbol printed before an amount. */
    prefix: string
  }
  /** One entry per offered extension. */
  pricing: TldPriceEntry[]
}
