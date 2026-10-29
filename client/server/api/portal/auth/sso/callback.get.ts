/**
 * GET /api/portal/auth/sso/callback?code=<code>&state=<state>
 *
 * Exchanges the authorization code for tokens, keeps them in the server-side session store (the
 * browser receives only the opaque session cookie), then redirects back.
 *
 * When the identity provider answers with an error instead of a code (a silent `prompt=none`
 * probe that found no session answers `login_required`), the visitor is returned to the page they
 * were on with the sign-in prompt flag (`?signin=required`), so the page says they are not signed
 * in and offers the sign-in, instead of quietly rendering as a guest.
 *
 * @module server/api/portal/auth/sso/callback.get
 */

import { timingSafeEqual } from 'node:crypto'
import { CLIENT_DASHBOARD_PATH } from '~/constants/clientRoutes'
import { SessionCookie } from '~/constants/sessionCookie'
import { cookieOptions } from '~/server/utils/auth-cookie'
import { startSession } from '~/server/utils/portal-session'
import type { SessionTokens } from '~/server/utils/portal-session'
import { readQueryString } from '~/server/utils/readQueryString'
import { requireSsoSetting } from '~/server/utils/requireSsoSetting'
import { isInternalPath, withSignInPrompt } from '~/utils/returnPath'
import { HttpMethod } from '~/types/httpmethod'

/** The SSO token endpoint's reply (OAuth field names, as the standard spells them). */
interface SsoTokenResponse {
  /** Access token for the portal session. */
  access_token: string
  /** Refresh token, when the SSO issued one. */
  refresh_token?: string
  /** Lifetime of the access token, in seconds. */
  expires_in: number
  /** Token type, always `Bearer`. */
  token_type: string
}

/** The members of a failed token exchange this route reads. */
interface TokenExchangeFailure {
  /** The SSO's error body. */
  data?: TokenExchangeFailureBody
  /** HTTP status of the failed exchange. */
  statusCode?: number
}

/** The SSO's OAuth error body. */
interface TokenExchangeFailureBody {
  /** The RFC 6749 error identifier, e.g. `invalid_grant`. */
  error?: string
}

/**
 * Compares the stored anti-forgery state with the returned one in constant time.
 *
 * @param stored - The state kept in the cookie.
 * @param returned - The state the provider sent back.
 * @returns True when both are present and equal.
 */
const statesMatch = (stored: string | undefined, returned: string | undefined): boolean => {
  if (!stored || !returned) {
    return false
  }
  const storedBytes = Buffer.from(stored)
  const returnedBytes = Buffer.from(returned)
  return storedBytes.length === returnedBytes.length && timingSafeEqual(storedBytes, returnedBytes)
}

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig()
  const query = getQuery(event)
  // A repeated key is refused (400) rather than read as whichever value comes first.
  const error = readQueryString(query.error)
  const code = readQueryString(query.code)
  const returnedState = readQueryString(query.state)
  // Fail closed: the token exchange sends the callback address as `redirect_uri`, so an empty one can never succeed.
  const redirectUri = requireSsoSetting(config.ssoCallbackUrl)

  // The provider answered with an error (a silent probe with no session answers `login_required`):
  // back to the page the visitor was on, flagged so it shows the sign-in prompt.
  // `sso_silent_tried` is deliberately left in place: it is the loop guard, and clearing
  // it here would send the very next page render straight back through /authorize. It
  // carries a short TTL of its own, so a later visit still gets a fresh session probe.
  if (error) {
    deleteCookie(event, SessionCookie.PkceVerifier, cookieOptions(0))
    deleteCookie(event, SessionCookie.OauthState, cookieOptions(0))
    const remembered = getCookie(event, SessionCookie.SsoSilentReturn) || getCookie(event, SessionCookie.PostLoginRedirect)
    deleteCookie(event, SessionCookie.SsoSilentReturn, cookieOptions(0))
    deleteCookie(event, SessionCookie.PostLoginRedirect, cookieOptions(0))
    const returnTo = isInternalPath(remembered) ? remembered : '/'
    return sendRedirect(event, withSignInPrompt(returnTo), 302)
  }

  if (!code) {
    throw createError({ statusCode: 400, data: { code: 'OAUTH_CODE_MISSING' } })
  }

  // Validate state to prevent CSRF
  const storedState = getCookie(event, SessionCookie.OauthState)
  if (!statesMatch(storedState, returnedState)) {
    throw createError({ statusCode: 400, data: { code: 'OAUTH_STATE_INVALID' } })
  }
  deleteCookie(event, SessionCookie.OauthState, cookieOptions(0))

  const codeVerifier = getCookie(event, SessionCookie.PkceVerifier)
  if (!codeVerifier) {
    throw createError({ statusCode: 400, data: { code: 'OAUTH_VERIFIER_MISSING' } })
  }

  // Clear the PKCE cookie immediately
  deleteCookie(event, SessionCookie.PkceVerifier, cookieOptions(0))

  // Exchange code for tokens (server-to-server, uses internal SSO URL)
  let tokenResponse: SsoTokenResponse

  // Bound to a plain `string` and given an explicit response type. Handed a template literal
  // with no generic, Nitro tries to infer the reply by matching this URL against its own route
  // table and recurses until TypeScript gives up with TS2321: for the SSO's token endpoint,
  // which matches no Nitro route. The shape is the one declared above.
  const tokenUrl: string = `${config.ssoUrl}/connect/token`

  try {
    tokenResponse = await $fetch<typeof tokenResponse>(tokenUrl, {
      method: HttpMethod.Post,
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      body: new URLSearchParams({
        grant_type: 'authorization_code',
        code,
        redirect_uri: redirectUri,
        client_id: config.ssoClientId,
        client_secret: config.ssoClientSecret,
        code_verifier: codeVerifier,
      }).toString(),
    })
  } catch (err: unknown) {
    // Allow-listed legacy-field read (scripts/check-no-legacy-error-fields.mjs): this `error` is the
    // SSO's OAuth reply (`invalid_grant` and the like), an RFC 6749 identifier, not a field of the hostpanel
    // backend's error body. It is forwarded as a code; the BFF writes no sentence.
    const e = err as TokenExchangeFailure
    throw createError({
      statusCode: 401,
      data: { code: e?.data?.error ?? 'TOKEN_EXCHANGE_FAILED' }, // legacy-gate: allow: the SSO's OAuth reply
    })
  }

  const { access_token, refresh_token, expires_in } = tokenResponse

  // The tokens stay on the server. A refresh token can be larger than the 4 KB a browser keeps per
  // cookie, and written as one the browser would drop it and end the session with the access
  // token.
  const tokens: SessionTokens = { accessToken: access_token }
  if (refresh_token) {
    tokens.refreshToken = refresh_token
  }
  if (expires_in !== undefined) {
    tokens.expiresIn = expires_in
  }
  await startSession(event, tokens)

  // No readable identity cookie is written for the header widget: it asks the sign-on
  // provider directly, through the /accounts-embed iframe, and the provider's own remembered
  // list already includes this account by the time it redirects back here. Such a cookie
  // would publish the user's identity to every script on this origin, and to anything
  // logging request headers.

  // If came from silent SSO on a public page: return there (now logged in)
  deleteCookie(event, SessionCookie.SsoSilentTried, cookieOptions(0))
  const silentReturn = getCookie(event, SessionCookie.SsoSilentReturn)
  deleteCookie(event, SessionCookie.SsoSilentReturn, cookieOptions(0))

  // post_login_redirect set by authorize.get.ts: return to the page user was on
  // (e.g. "Add another account" should bring back to current page, not dashboard)
  const postLoginRedirect = getCookie(event, SessionCookie.PostLoginRedirect)
  deleteCookie(event, SessionCookie.PostLoginRedirect, cookieOptions(0))

  // Both cookies were written from request input, so each is checked again before it is followed.
  const returnTo = [silentReturn, postLoginRedirect].find(isInternalPath) ?? CLIENT_DASHBOARD_PATH
  return sendRedirect(event, returnTo, 302)
})
