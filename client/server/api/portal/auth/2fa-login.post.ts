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
import { HttpMethod } from '~/types/httpmethod'
import { readBodyObject } from '~/server/utils/readBodyObject'

/** The `data.code` of the 502 answered when the backend completes the sign-in without an access token. */
export const TWO_FACTOR_NO_TOKEN = 'TWO_FACTOR_NO_TOKEN'

/** What the backend answers a completed two-factor sign-in with. */
interface TwoFactorLoginResult {
  /** Access token, kept in the server-side session; guarded anyway, as a reply without one is a backend fault. */
  accessToken?: string
  /** Lifetime of the access token, in seconds. */
  expiresIn?: number
}

/**
 * Completes the two-factor sign-in and sets the session cookies.
 */
export default defineEventHandler(async (event) => {
  const body = await readBodyObject(event)

  const response = await internalApiCall<TwoFactorLoginResult>(
    event,
    '/auth/2fa-login',
    { method: HttpMethod.Post, body, anonymous: true }
  )

  // No token means no session can start; answering success would leave the person signed out with no explanation.
  if (!response.accessToken) {
    throw createError({ statusCode: 502, data: { code: TWO_FACTOR_NO_TOKEN } })
  }

  await startSession(event, response.expiresIn === undefined
    ? { accessToken: response.accessToken }
    : { accessToken: response.accessToken, expiresIn: response.expiresIn })

  return { success: true }
})
