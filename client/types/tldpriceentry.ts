/**
 * One extension's entry of the TLD price table, as `GET /api/portal/public/tld-pricing` returns it.
 *
 * @module types/tldpriceentry
 */

import type { TldPeriodPrice } from '~/types/tldperiodprice'

/** The prices and tags of one offered extension. */
export interface TldPriceEntry {
  /** The extension the prices are for, e.g. "com". */
  tld: string
  /**
   * ISO 4217 code this TLD's own sell prices are set in. An order can only be placed in
   * this currency: the figures below may be converted for display when it differs from
   * the table's currency, but the frontend refuses to add such a TLD to the cart.
   */
  sellCurrency: string
  /** Registration price per period. */
  register: TldPeriodPrice[]
  /** Transfer price per period; falls back to registration when absent. */
  transfer?: TldPeriodPrice[]
  /** Renewal price per period; falls back to registration when absent. */
  renew?: TldPeriodPrice[]
  /** Category tags the backend assigns, used for filtering. */
  categories?: string[]
}
