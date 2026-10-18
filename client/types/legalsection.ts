/**
 * One numbered or icon-led section of a legal document (terms, privacy, cookies, refunds, acceptable use).
 *
 * @module types/legalsection
 */

import type { LegalCard } from '~/types/legalcard'
import type { LegalLink } from '~/types/legallink'

/** One numbered or icon-led section of a legal document (terms, privacy, cookies, refunds, acceptable use). */
export interface LegalSection {
  /** Stable key of the section; also the suffix of its `data-testid`. */
  id: string
  /** Heading of the section. */
  title: string
  /** Name of a Nuxt Icon shown beside the heading; without one the section's number is shown. */
  icon?: string
  /** Body paragraphs, in reading order. */
  paragraphs?: string[]
  /** Bulleted entries shown after the paragraphs. */
  items?: string[]
  /** Draws {@link items} with check marks instead of bullets. */
  checklist?: boolean
  /** Titled blocks shown after the entries. */
  cards?: LegalCard[]
  /** A highlighted remark shown after the blocks. */
  note?: string
  /** A link shown last. */
  link?: LegalLink
}
