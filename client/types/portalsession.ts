/**
 * One signed-in portal session as the Nitro server keeps it.
 *
 * The tokens never reach the browser: the browser holds only the opaque id this record is stored
 * under (`SessionCookie.Session`). A refresh token can be several kilobytes, more than one cookie
 * may carry, which is why it lives here and not in a cookie.
 *
 * @module types/portalsession
 */

/** The tokens and owner of one signed-in session. */
export interface PortalSession {
  /** The access token sent to the backend as the bearer. */
  accessToken: string
  /** The refresh token exchanged for a new access token; absent when the provider issued none. */
  refreshToken?: string
  /** The `sub` claim of the access token, so every session of one person can be ended together. */
  subject?: string
  /** When the access token expires, in seconds since the epoch; absent when it states no expiry. */
  accessTokenExpiresAt?: number
}
