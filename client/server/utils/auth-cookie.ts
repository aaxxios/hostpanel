/**
 * Shared lifetime for the `refresh_token` cookie.
 *
 * This value has to track innovayse-sso's own refresh-token lifetime, set in
 * `Innovayse.SSO.API/Program.cs` as `SetRefreshTokenLifetime(TimeSpan.FromDays(30))`.
 * It had drifted: every one of the three places that store a refresh token wrote its
 * own `60 * 60 * 24 * 7`, so on the eighth day the browser dropped a credential the
 * SSO would still have accepted for another three weeks, and the user was sent back
 * through a sign-in they did not need.
 *
 * Defining it once is the point — three copies are what let it drift in the first
 * place. If the SSO's lifetime changes, change it here and nowhere else.
 */
export const REFRESH_TOKEN_MAX_AGE = 60 * 60 * 24 * 30

/**
 * Reads the `exp` claim (seconds since the epoch) of a JWT without verifying it.
 *
 * The payload is only decoded, never trusted for access: the API issued the token, and this is
 * used solely to size the browser cookie that carries it.
 *
 * @param token - A compact JWT.
 * @returns The `exp` claim, or null when the token is not a JWT or carries no numeric `exp`.
 */
const readTokenExpiry = (token: string): number | null => {
  const payload = token.split('.')[1]
  if (!payload) {
    return null
  }
  try {
    const json = Buffer.from(payload.replace(/-/g, '+').replace(/_/g, '/'), 'base64').toString('utf8')
    const exp = (JSON.parse(json) as { exp?: unknown }).exp
    return typeof exp === 'number' && Number.isFinite(exp) ? exp : null
  } catch {
    return null
  }
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
