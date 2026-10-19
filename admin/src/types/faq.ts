/**
 * A frequently asked question shown on the storefront, its per-locale text, and the payload that
 * writes one.
 */
import type { FaqCategory } from '@/types/storefrontenums'
import type { StorefrontTranslations } from '@/types/storefront'

/** The translated part of a FAQ entry. */
export interface FaqTranslation {
  /** The question. */
  question: string
  /** The answer. */
  answer: string
}

/** Payload for creating or updating a FAQ entry. */
export interface FaqPayload {
  /** Topic the entry belongs to. */
  category: FaqCategory
  /** Display order. */
  sortOrder: number
  /** Whether the entry is shown. */
  isActive: boolean
  /** Question and answer per locale, keyed by locale code; a locale left out is removed on update, at least one is required. */
  translations: StorefrontTranslations<FaqTranslation>
}

/** A FAQ entry as the admin API returns it. */
export interface Faq extends FaqPayload {
  /** FAQ entry ID. */
  id: number
  /** When the entry was created (ISO 8601). */
  createdAt: string
}
