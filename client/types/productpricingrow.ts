/**
 * One plan of a product family, as a family card and the comparison table show it.
 *
 * @module types/productpricingrow
 */

import type { PricingCycle } from '~/types/pricingcycle'

/** One plan row: its tier name, its first offered price and the feature lines parsed from its description. */
export interface ProductPricingRow {
  /** Catalogue product id of the plan. */
  id: number
  /** Plan name, e.g. "Professional". */
  tier: string
  /** URL slug of the plan, or an empty string when it has none. */
  slug: string
  /** The first offered price, formatted in the payer's currency; empty when the plan has no price. */
  price: string
  /** The same price as a number (0 when none). */
  amount: number
  /** ISO 4217 code the price is in; empty until the payer's currency resolves. */
  currency: string
  /** The cycle the first offered price is for. */
  billingCycle: PricingCycle
  /** Feature lines parsed from the plan description. */
  features: string[]
}
