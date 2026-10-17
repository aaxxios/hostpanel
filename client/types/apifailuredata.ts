/**
 * The `data` the BFF attaches to a refusal (`server/utils/api.ts` `backendError`).
 *
 * @module types/apifailuredata
 */

/** The `data` the BFF attaches to a refusal (`server/utils/api.ts` `backendError`). */
export interface ApiFailureData {
  /** The backend's SCREAMING_SNAKE code, or null when it sent none. */
  code?: string | null
  /** The backend's localized summary, or null. */
  title?: string | null
  /** Field name to messages of a 400 `VALIDATION_ERROR`, or null. */
  errors?: Record<string, string[]> | null
}
