/**
 * One plan column of the products-page comparison table.
 *
 * @module types/comparisonplan
 */

import type { PricingCycle } from '~/types/pricingcycle'

/** One plan column: its name, first offered price and whether the operator recommends it. */
export interface ComparisonPlan {
  /** Catalogue product id, also the key of its values in the group's rows. */
  id: number
  /** Display name, e.g. "Business". */
  name: string
  /** First offered price formatted in the payer's currency; empty when the plan has no price. */
  price: string
  /** The cycle the price is for. */
  billingCycle: PricingCycle
  /** True for the plan the operator marked as recommended; its column wears the badge. */
  isFeatured: boolean
}
