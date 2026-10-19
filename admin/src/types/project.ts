/**
 * A portfolio project shown on the storefront, its per-locale text, and the payload that writes
 * one.
 */
import type { StorefrontTranslations } from '@/types/storefront'

/** The translated part of a project. */
export interface ProjectTranslation {
  /** Project title. */
  title: string
  /** One-line summary; empty when none is given. */
  summary: string
  /** Full description; empty when none is given. */
  description: string
}

/** Payload for creating or updating a project. */
export interface ProjectPayload {
  /** URL-safe identifier used in the project's address. */
  slug: string
  /** Gallery image URLs, first one is the cover; at most 10. */
  imageUrls: string[]
  /** Link to the live project. */
  url: string | null
  /** Display order. */
  sortOrder: number
  /** Whether the project is shown. */
  isActive: boolean
  /** Text per locale, keyed by locale code; a locale left out is removed on update, at least one is required. */
  translations: StorefrontTranslations<ProjectTranslation>
}

/** A project as the admin API returns it. */
export interface Project extends ProjectPayload {
  /** Project ID. */
  id: number
  /** When the project was created (ISO 8601). */
  createdAt: string
}
