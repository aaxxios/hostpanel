/**
 * One product, as its detail page shows it.
 *
 * @module types/productdetail
 */

import type { ProductPlan } from '~/types/productplan'

/** One product family: its visual config, its summary and its plans. */
export interface ProductDetail {
  /** Config key (slug) of the product. */
  id: string
  /** Catalogue group id, which the family's specification lines are read by. */
  gid: number
  /** Catalogue product id of the product the address named. */
  pid: number | undefined
  /** Display name of the family (its group), never a plan's. */
  name: string
  /** Full icon name, e.g. `mdi:server`. */
  icon: string
  /** Accent colour as a hex string. */
  color: string
  /** Demo link, when the product has one. */
  demoUrl?: string
  /** The family's catalogue page. */
  learnMoreUrl: string
  /** The family's own description (its group's), tags stripped; a plan's blurb only when the group has none. */
  description: string
  /** The key bullets of the family: its parent product's feature lines, else those of its cheapest plan that lists any. */
  features: string[]
  /** The plans (tiers) of the family, in display order. */
  plans: ProductPlan[]
}
