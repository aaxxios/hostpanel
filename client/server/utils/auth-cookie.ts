/**
 * Lifetime of a session that holds a refresh token, in seconds: the lifetime the identity
 * provider grants a refresh token (thirty days).
 *
 * It had drifted once: every place that stored a refresh token wrote its own
 * `60 * 60 * 24 * 7`, so on the eighth day a credential the provider would still have accepted
 * for another three weeks was thrown away, and the user was sent back through a sign-in they did
 * not need. Defining it once is the point. If the provider's lifetime changes, change it here and
 * nowhere else.
 */
export const REFRESH_TOKEN_MAX_AGE = 60 * 60 * 24 * 30

/**
 * Decodes the payload of a JWT without verifying it.
 *
 * The payload is only decoded, never trusted for access: the token was issued to this server by
 * the API or the identity provider, and its claims are read solely to size a session and to know
 * whose session it is.
 *
 * @param token - A compact JWT.
 * @returns The claims, or null when the token is not a JWT.
 */
export const readTokenPayload = (token: string): Record<string, unknown> | null => {
  const payload = token.split('.')[1]
  if (!payload) {
    return null
  }
  try {
    const json = Buffer.from(payload.replace(/-/g, '+').replace(/_/g, '/'), 'base64').toString('utf8')
    const claims = JSON.parse(json) as unknown
    return claims !== null && typeof claims === 'object' ? (claims as Record<string, unknown>) : null
  } catch {
    return null
  }
}

/**
 * Reads the `exp` claim (seconds since the epoch) of a JWT without verifying it.
 *
 * @param token - A compact JWT.
 * @returns The `exp` claim, or null when the token is not a JWT or carries no numeric `exp`.
 */
export const readTokenExpiry = (token: string): number | null => {
  const exp = readTokenPayload(token)?.['exp']
  return typeof exp === 'number' && Number.isFinite(exp) ? exp : null
}

/**
 * Cookie lifetime for an access token, taken from the token's own `exp` claim so the cookie
 * follows the API's configured session lifetime instead of a hard-coded number.
 *
 * @param token - The access token the API issued.
 * @param nowSeconds - Current time in seconds since the epoch; defaults to the clock.
 * @returns Seconds until `exp` (never negative), or undefined -- a session cookie -- when `exp` is missing.
 */
export const authCookieMaxAge = (token: string, nowSeconds: number = Math.floor(Date.now() / 1000)): number | undefined => {
  const exp = readTokenExpiry(token)
  if (exp === null) {
    return undefined
  }
  return Math.max(0, exp - nowSeconds)
}

/** Lifetime of the readable `authed` flag cookie, in seconds (seven days). */
export const AUTHED_FLAG_MAX_AGE = 60 * 60 * 24 * 7

/** Lifetime, in seconds, of the short cookies that carry a single sign-in round trip (PKCE verifier, state, return address). */
export const SSO_FLOW_COOKIE_MAX_AGE = 60 * 5

/** Lifetime, in seconds, of the loop guard and return address of the silent sign-in attempt. */
export const SSO_SILENT_COOKIE_MAX_AGE = 30

/** Access-token lifetime, in seconds, assumed when the identity provider does not state one (15 minutes). */
export const ACCESS_TOKEN_FALLBACK_MAX_AGE = 60 * 15
