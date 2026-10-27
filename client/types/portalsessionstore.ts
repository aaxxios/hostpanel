/**
 * Where the Nitro server keeps the portal sessions.
 *
 * Two implementations live in `server/utils/session-store.ts`: Redis for every deployment, and an
 * in-process map for a single-process run without Redis (unit tests, the e2e build, `nuxt dev`).
 *
 * @module types/portalsessionstore
 */

import type { PortalSession } from './portalsession'

/** Reads, writes and deletes portal sessions by their opaque id. */
export interface PortalSessionStore {
  /**
   * Reads a session.
   *
   * @param id - The opaque session id from the browser cookie.
   * @returns The session, or null when it does not exist or has expired.
   */
  get: (id: string) => Promise<PortalSession | null>
  /**
   * Writes a session, replacing any previous value under the same id.
   *
   * @param id - The opaque session id.
   * @param session - The tokens and owner to keep.
   * @param ttlSeconds - How long the entry lives, in seconds.
   * @returns Resolves once the entry is stored.
   */
  set: (id: string, session: PortalSession, ttlSeconds: number) => Promise<void>
  /**
   * Deletes a session. Deleting one that does not exist is not an error.
   *
   * @param id - The opaque session id.
   * @returns Resolves once the entry is gone.
   */
  delete: (id: string) => Promise<void>
}
