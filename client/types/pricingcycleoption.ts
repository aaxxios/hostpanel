/**
 * A billing cycle a plan can be bought for, with its label and price already formatted.
 *
 * @module types/pricingcycleoption
 */

import type { PricingCycle } from '~/types/pricingcycle'

/** One selectable billing cycle of a plan. */
export interface PricingCycleOption {
  /** The cycle. */
  cycle: PricingCycle
  /** Its name in the reader's language, e.g. "Annually". */
  label: string
  /** The plan's price for the cycle, formatted in the payer's currency. */
  price: string
}
