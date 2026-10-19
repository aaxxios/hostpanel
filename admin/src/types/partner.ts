/**
 * A partner logo shown on the storefront, its per-locale name, and the payload that writes one.
 */
import type { StorefrontTranslations } from '@/types/storefront'

/** The translated part of a partner. */
export interface PartnerTranslation {
  /** Partner name. */
  name: string
}

/** Payload for creating or updating a partner. */
export interface PartnerPayload {
  /** Logo image URL; null when none is stored. */
  logoUrl: string | null
  /** Partner website; null when none is stored. */
  url: string | null
  /** Display order. */
  sortOrder: number
  /** Whether the partner is shown. */
  isActive: boolean
  /** Name per locale, keyed by locale code; a locale left out is removed on update, at least one is required. */
  translations: StorefrontTranslations<PartnerTranslation>
}

/** A partner as the admin API returns it. */
export interface Partner extends PartnerPayload {
  /** Partner ID. */
  id: number
  /** When the partner was created (ISO 8601). */
  createdAt: string
}
