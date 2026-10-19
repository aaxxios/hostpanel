/**
 * POST /api/portal/auth/2fa-login
 *
 * Completes a two-factor sign-in with the pending token and the authenticator code, then stores
 * the session in httpOnly cookies. Anonymous (no session yet) but routed through
 * `internalApiCall` so `X-Language` is forwarded and a refusal keeps the backend's `detail`.
 *
 * @module server/api/portal/auth/2fa-login.post
 */

import { SessionCookie } from '~/constants/sessionCookie'
import { ACCESS_TOKEN_FALLBACK_MAX_AGE, AUTHED_FLAG_MAX_AGE } from '~/server/utils/auth-cookie'

/** What the backend answers a completed two-factor sign-in with. */
interface TwoFactorLoginResult {
  /** Access token stored in the httpOnly `auth_token` cookie. */
  accessToken: string
  /** Lifetime of the access token, in seconds. */
  expiresIn: number
}

/**
 * Completes the two-factor sign-in and sets the session cookies.
 */
export default defineEventHandler(async (event) => {
  const body = await readBody(event)

  const response = await internalApiCall<TwoFactorLoginResult>(
    event,
    '/auth/2fa-login',
    { method: 'POST', body, anonymous: true }
  )

  setCookie(event, SessionCookie.AuthToken, response.accessToken, {
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'lax',
    maxAge: response.expiresIn ?? ACCESS_TOKEN_FALLBACK_MAX_AGE,
    path: '/',
  })
  setCookie(event, SessionCookie.Authed, '1', {
    httpOnly: false,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'lax',
    maxAge: AUTHED_FLAG_MAX_AGE,
    path: '/',
  })

  return { success: true }
})
