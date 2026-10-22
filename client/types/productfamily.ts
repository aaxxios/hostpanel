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
  /** Full icon name, e.g. `mdi:server`. */
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
  /** Service type the family sells, as `ProductType` serialises; picks the generated cover. Absent when unknown. */
  type?: string | null
  /** Cover image an operator uploaded for the group; replaces the generated cover when set. */
  image?: string | null
  /** True when one of the family's plans is its group's recommended plan; the card wears a badge. */
  featured?: boolean
}
