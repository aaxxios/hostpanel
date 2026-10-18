/**
 * The subset of a catalogue product the classic hosting page reads.
 *
 * `tagline` and `shortdescription` are deliberately absent: `ProductDto` sends neither, so the
 * prose that used to be expected from them lives at the head of `description`, and the page pulls
 * it out with `parseDescription`.
 *
 * @module types/hostingplan
 */

import type { PortalProductPricing } from '~/types/portalproductpricing'

/** One hosting plan as the classic hosting page renders it. */
export interface HostingPlan {
  /** Catalogue row id, absent on some payloads. */
  id?: number
  /** Product id - the value the cart and order flow key on. */
  pid: number
  /** Display name as the operator wrote it. */
  name: string
  /** Operator-authored HTML: leading prose, then `<br />`-separated feature lines. */
  description: string
  /** `'on'` when the operator flagged the plan as the featured one. */
  is_featured?: string
  /** The plan's price in the caller's own currency; null fields when not offered. */
  pricing?: PortalProductPricing | null
}
