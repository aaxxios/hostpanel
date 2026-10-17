/**
 * One product-family card of the products page.
 *
 * @module types/productfamily
 */

import type { ProductPricingRow } from '~/types/productpricingrow'

/** One product-family card: the static domains entry or a catalogue group. */
export interface ProductFamily {
  /** Config key (slug) of the family, or `domains` for the static card. */
  id: string
  /** Catalogue group id; null for the static domains card. */
  gid: number | null
  /** Icon name (without the `mdi:` prefix). */
  icon: string
  /** Accent colour as a hex string. */
  color: string
  /** Demo link, when the family has one. */
  demoUrl?: string
  /** Where "learn more" leads. */
  learnMoreUrl: string
  /** Display name. */
  name: string
  /** Short tagline; empty for catalogue families. */
  tagline: string
  /** Summary paragraph. */
  description: string
  /** Aggregated feature lines. */
  features: string[]
  /** One pricing row per plan. */
  pricing: ProductPricingRow[]
}
