/**
 * A hosting plan as nova's pricing cards render it.
 *
 * @module templates/nova/types/novaplan
 */

import type { PlanCard } from '~/types/plancard'

/**
 * A hosting plan as nova's pricing cards render it.
 *
 * Built from the {@link PlanCard} shape `stores/catalog.ts` `planCards` returns, with the two
 * decisions the cards need layered on: which plan carries the "most popular"
 * badge and what the yearly saving is. Both are derived, never fetched — see
 * `pricing.ts`, which is where they are computed and tested.
 */
export interface NovaPlan extends PlanCard {
  /** Whether this plan carries the badge. At most one plan in a list has it. */
  popular: boolean
  /**
   * Whole-percent saving of the annual price against twelve monthly ones, or
   * `null` when the two prices do not support the claim. Never rendered unless
   * it is a positive number.
   */
  discountPercent: number | null
}
