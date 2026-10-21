/**
 * A server location shown on the storefront, its per-locale text, and the payload that writes one.
 */
import type { StorefrontTranslations } from '@/types/storefront'

/** The translated part of a server location. */
export interface ServerLocationTranslation {
  /** Location name. */
  name: string
  /** City; empty or null when none is written. */
  city: string | null
  /** Short latency note; empty or null when none is written. */
  latencyNote: string | null
}

/** Payload for creating or updating a server location. */
export interface ServerLocationPayload {
  /** ISO 3166 alpha-2 country code, upper case. */
  countryCode: string
  /** Display order. */
  sortOrder: number
  /** Whether the location is shown. */
  isActive: boolean
  /** Text per locale, keyed by locale code; a locale left out is removed on update, at least one is required. */
  translations: StorefrontTranslations<ServerLocationTranslation>
}

/** A server location as the admin API returns it. */
export interface ServerLocation extends ServerLocationPayload {
  /** Location ID. */
  id: number
}
