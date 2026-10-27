/**
 * POST /api/portal/auth/login
 *
 * Signs in against the C# API (local-auth mode) and starts the server-side session (the browser receives only the opaque session cookie). The
 * call is anonymous -- no session exists yet -- but still goes through `internalApiCall`, so the
 * reader's `X-Language` reaches the backend and a refusal arrives as the backend's own
 * `detail` / `code` rather than a raw upstream error.
 *
 * @module server/api/portal/auth/login.post
 */

import { startSession } from '~/server/utils/portal-session'

/** What the backend answers a sign-in with. */
interface LoginResult {
  /** Access token; absent when a second factor is still required. */
  accessToken?: string
  /** Lifetime of the access token, in seconds. */
  expiresIn?: number
  /** True when the account has two-factor on and the sign-in is not complete yet. */
  twoFactorRequired?: boolean
  /** Short-lived token that the `2fa-login` call presents together with the code. */
  pendingToken?: string
}

/**
 * Signs in and sets the session cookies, or passes a two-factor challenge through.
 */
export default defineEventHandler(async (event) => {
  const body = await readBody(event)

  const response = await internalApiCall<LoginResult>(
    event,
    '/auth/login',
    { method: 'POST', body, anonymous: true }
  )

  // 2FA required — pass through without setting cookies
  if (response.twoFactorRequired) {
    return response
  }

  // Start the session; the token stays on the server
  if (response.accessToken) {
    await startSession(event, response.expiresIn === undefined
      ? { accessToken: response.accessToken }
      : { accessToken: response.accessToken, expiresIn: response.expiresIn })
  }

  return { success: true }
})
