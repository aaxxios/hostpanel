/**
 * One product, as its detail page shows it.
 *
 * @module types/productdetail
 */

import type { ProductPlan } from '~/types/productplan'

/** One product: its visual config, its summary and its plans. */
export interface ProductDetail {
  /** Config key (slug) of the product. */
  id: string
  /** Catalogue product id. */
  pid: number | undefined
  /** Display name. */
  name: string
  /** Icon name (without the `mdi:` prefix). */
  icon: string
  /** Accent colour as a hex string. */
  color: string
  /** Demo link, when the product has one. */
  demoUrl?: string
  /** The family's catalogue page. */
  learnMoreUrl: string
  /** Summary paragraph, tags stripped. */
  description: string
  /** Feature lines parsed from the description. */
  features: string[]
  /** The plans (tiers) of the product. */
  plans: ProductPlan[]
}
