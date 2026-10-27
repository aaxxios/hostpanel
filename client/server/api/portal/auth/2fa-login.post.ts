/**
 * POST /api/portal/auth/2fa-login
 *
 * Completes a two-factor sign-in with the pending token and the authenticator code, then starts
 * the server-side session (the browser receives only the opaque session cookie). Anonymous (no session yet) but routed through
 * `internalApiCall` so `X-Language` is forwarded and a refusal keeps the backend's `detail`.
 *
 * @module server/api/portal/auth/2fa-login.post
 */

import { startSession } from '~/server/utils/portal-session'

/** What the backend answers a completed two-factor sign-in with. */
interface TwoFactorLoginResult {
  /** Access token, kept in the server-side session. */
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

  await startSession(event, { accessToken: response.accessToken, expiresIn: response.expiresIn })

  return { success: true }
})
