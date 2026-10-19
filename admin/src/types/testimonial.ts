/**
 * A customer testimonial shown on the storefront, its per-locale text, and the payload that
 * writes one.
 */
import type { StorefrontTranslations } from '@/types/storefront'

/** The translated part of a testimonial. */
export interface TestimonialTranslation {
  /** Customer role in the company; empty when none is given. */
  role: string
  /** What the customer said. */
  quote: string
}

/** Payload for creating or updating a testimonial. */
export interface TestimonialPayload {
  /** Customer name. */
  name: string
  /** Customer company; null when none is stored. */
  company: string | null
  /** Avatar image URL; null when none is stored. */
  avatarUrl: string | null
  /** Rating from 1 to 5. */
  rating: number
  /** Display order. */
  sortOrder: number
  /** Whether the testimonial is shown. */
  isActive: boolean
  /** Role and quote per locale, keyed by locale code; a locale left out is removed on update, at least one is required. */
  translations: StorefrontTranslations<TestimonialTranslation>
}

/** A testimonial as the admin API returns it. */
export interface Testimonial extends TestimonialPayload {
  /** Testimonial ID. */
  id: number
  /** When the testimonial was created (ISO 8601). */
  createdAt: string
}
