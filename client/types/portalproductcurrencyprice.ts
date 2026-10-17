/**
 * One product's price in one specific currency, as `ProductDto.prices` carries them.
 *
 * @module types/portalproductcurrencyprice
 */

/** One product's price in one specific currency, as `ProductDto.prices` carries them. */
export interface PortalProductCurrencyPrice {
  /** ISO 4217 alpha code this row is priced in. */
  currencyCode: string
  /** Monthly price in this currency, or null when not offered. */
  monthly: number | null
  /** Annual price in this currency, or null when not offered. */
  annual: number | null
}
