/**
 * One plan on a product's detail page: a tier with prices for both cycles.
 *
 * @module types/productplan
 */

import type { PortalProductPricing } from '~/types/portalproductpricing'

/** One plan on a product's detail page: a tier with prices for both cycles. */
export interface ProductPlan {
  /** Catalogue product id, which the specification lines of the comparison are keyed by. */
  id: number
  /** Order id of the plan, the one the cart and the order link carry. */
  pid: number
  /** Plan name, e.g. "Starter". */
  tier: string
  /** The plan's prices in the payer's currency; null when it carries none. */
  pricing: PortalProductPricing | null
  /** Leading prose of the plan description, tags stripped; empty when the copy is bullets only. */
  description: string
  /** Feature lines parsed from the plan description. */
  features: string[]
  /** True for the plan the operator marked as recommended. */
  isFeatured: boolean
}
