/**
 * GET /api/portal/auth/sso/callback?code=...
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

import { CLIENT_DASHBOARD_PATH } from '~/constants/clientRoutes'
import { SessionCookie } from '~/constants/sessionCookie'
import { startSession } from '~/server/utils/portal-session'
import type { SessionTokens } from '~/server/utils/portal-session'
import { requireSsoSetting } from '~/server/utils/requireSsoSetting'
import { isInternalPath, withSignInPrompt } from '~/utils/returnPath'

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

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig()
  const query = getQuery(event)
  // Fail closed: the token exchange sends the callback address as `redirect_uri`, so an empty one can never succeed.
  const redirectUri = requireSsoSetting(config.ssoCallbackUrl)

  // The provider answered with an error (a silent probe with no session answers `login_required`):
  // back to the page the visitor was on, flagged so it shows the sign-in prompt.
  // `sso_silent_tried` is deliberately left in place: it is the loop guard, and clearing
  // it here would send the very next page render straight back through /authorize. It
  // carries a short TTL of its own, so a later visit still gets a fresh session probe.
  const error = query.error as string | undefined
  if (error) {
    deleteCookie(event, SessionCookie.PkceVerifier, { path: '/' })
    deleteCookie(event, SessionCookie.OauthState, { path: '/' })
    const remembered = getCookie(event, SessionCookie.SsoSilentReturn) || getCookie(event, SessionCookie.PostLoginRedirect)
    deleteCookie(event, SessionCookie.SsoSilentReturn, { path: '/' })
    deleteCookie(event, SessionCookie.PostLoginRedirect, { path: '/' })
    const returnTo = isInternalPath(remembered) ? remembered : '/'
    return sendRedirect(event, withSignInPrompt(returnTo), 302)
  }

  const code = query.code as string | undefined
  if (!code) {
    throw createError({ statusCode: 400, data: { code: 'OAUTH_CODE_MISSING' } })
  }

  // Validate state to prevent CSRF
  const storedState = getCookie(event, SessionCookie.OauthState)
  const returnedState = query.state as string | undefined
  if (!storedState || storedState !== returnedState) {
    throw createError({ statusCode: 400, data: { code: 'OAUTH_STATE_INVALID' } })
  }
  deleteCookie(event, SessionCookie.OauthState, { path: '/' })

  const codeVerifier = getCookie(event, SessionCookie.PkceVerifier)
  if (!codeVerifier) {
    throw createError({ statusCode: 400, data: { code: 'OAUTH_VERIFIER_MISSING' } })
  }

  // Clear the PKCE cookie immediately
  deleteCookie(event, SessionCookie.PkceVerifier, { path: '/' })

  // Exchange code for tokens (server-to-server, uses internal SSO URL)
  let tokenResponse: SsoTokenResponse

  // Bound to a plain `string` and given an explicit response type. Handed a template literal
  // with no generic, Nitro tries to infer the reply by matching this URL against its own route
  // table and recurses until TypeScript gives up with TS2321 -- for the SSO's token endpoint,
  // which matches no Nitro route. The shape is the one declared above.
  const tokenUrl: string = `${config.ssoUrl}/connect/token`

  try {
    tokenResponse = await $fetch<typeof tokenResponse>(tokenUrl, {
      method: 'POST',
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
    // SSO's OAuth reply (`invalid_grant`, ...), an RFC 6749 identifier, not a field of the hostpanel
    // backend's error body. It is forwarded as a code; the BFF writes no sentence.
    const e = err as TokenExchangeFailure
    throw createError({
      statusCode: 401,
      data: { code: e?.data?.error ?? 'TOKEN_EXCHANGE_FAILED' }, // legacy-gate: allow -- the SSO's OAuth reply
    })
  }

  const { access_token, refresh_token, expires_in } = tokenResponse

  // The tokens stay on the server. A refresh token can be larger than the 4 KB a browser keeps per
  // cookie, and when it was written as one the browser dropped it and the session ended with the
  // access token.
  const tokens: SessionTokens = { accessToken: access_token }
  if (refresh_token) {
    tokens.refreshToken = refresh_token
  }
  if (expires_in !== undefined) {
    tokens.expiresIn = expires_in
  }
  await startSession(event, tokens)

  // This used to fetch /connect/userinfo and write the result — subject id, email, first
  // and last name — into a non-httpOnly `inno_pending_account` cookie, so the widget
  // could merge it into its innovayse_accounts localStorage list and show the account in
  // the switcher without a visit to app.innovayse.local first.
  //
  // Nothing consumes it any more. The widget asks the SSO directly, through the
  // /accounts-embed iframe, and the SSO's own remembered list already includes this
  // account by the time it redirects back here — so the cookie only published the user's
  // identity to every script on this origin, and to anything logging request headers,
  // for no remaining purpose. The widget still deletes it, which clears the ones
  // already sitting in browsers.

  // If came from silent SSO on a public page — return there (now logged in)
  deleteCookie(event, SessionCookie.SsoSilentTried, { path: '/' })
  const silentReturn = getCookie(event, SessionCookie.SsoSilentReturn)
  deleteCookie(event, SessionCookie.SsoSilentReturn, { path: '/' })

  // post_login_redirect set by authorize.get.ts — return to the page user was on
  // (e.g. "Add another account" should bring back to current page, not dashboard)
  const postLoginRedirect = getCookie(event, SessionCookie.PostLoginRedirect)
  deleteCookie(event, SessionCookie.PostLoginRedirect, { path: '/' })

  return sendRedirect(event, silentReturn || postLoginRedirect || CLIENT_DASHBOARD_PATH, 302)
})
