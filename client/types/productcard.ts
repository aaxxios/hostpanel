/**
 * One image card of the home page's products section.
 *
 * @module types/productcard
 */

/** One product card: a catalogue family or the static domains entry, with its visual config. */
export interface ProductCard {
  /** Config key (slug) of the family, or `domains` for the static card. */
  id: string
  /** Display name. */
  name: string
  /** Short tagline; empty for catalogue families, which have none. */
  tagline: string
  /** Summary paragraph. */
  description: string
  /** Up to three feature lines. */
  features: string[]
  /** Icon name without the `mdi:` prefix. */
  icon: string
  /** Accent colour as a hex string. */
  color: string
  /** Background image path; empty when the family has none. */
  image: string
  /** Demo link; empty when there is none. */
  demoUrl: string
  /** Where "learn more" leads. */
  learnMoreUrl: string
}
