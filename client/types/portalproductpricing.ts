/**
 * The product's price in the caller's own currency, as `ProductPricingDto` sends it.
 *
 * @module types/portalproductpricing
 */

/**
 * The product's price in the caller's own currency, as `ProductPricingDto` sends it.
 *
 * Both fields are `null` when the product carries no price in that currency at all — the
 * storefront hides such a product for the cycle that is null (`GET /products` without
 * `includeUnsellable` already excludes a product with no price in any currency, but a product
 * priced only annually, say, still needs the monthly card hidden).
 */
export interface PortalProductPricing {
  /** Monthly price in the caller's currency, or null when not offered. */
  monthly: number | null
  /** Annual price in the caller's currency, or null when not offered. */
  annual: number | null
}
