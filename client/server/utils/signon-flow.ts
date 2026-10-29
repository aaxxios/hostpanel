/**
 * The start of the external sign-on authorization-code flow, shared by every route that sends the
 * browser to the sign-on provider (`authorize.get.ts`, `switch.get.ts`).
 *
 * One place creates the PKCE pair and the `state`, binds them to this browser in short-lived
 * httpOnly cookies, and builds the authorize query. The callback later checks the returned `state`
 * and the PKCE proof against those cookies, so a route that only starts the flow never changes the
 * local session.
 *
 * @module server/utils/signon-flow
 */

import { randomBytes, createHash } from 'node:crypto'
import type { H3Event } from 'h3'
import { SessionCookie } from '~/constants/sessionCookie'
import { SSO_FLOW_COOKIE_MAX_AGE, cookieOptions } from '~/server/utils/auth-cookie'
import { requireSsoSetting } from '~/server/utils/requireSsoSetting'
import { isInternalPath } from '~/utils/returnPath'

/** The scopes the flow requests. */
const SIGNON_SCOPE = 'openid profile email offline_access'

/** The longest `login_hint` or account id passed on; the longest valid e-mail address is 254 characters. */
export const MAX_LOGIN_HINT_LENGTH = 254

/** A started flow: where the provider lives and the authorize query bound to this browser. */
export interface SignonFlow {
  /** The public address of the sign-on provider, never empty. */
  ssoPublicUrl: string
  /** The authorize query: client, redirect address, scope, PKCE challenge and `state`. */
  params: URLSearchParams
}

/**
 * Keeps a path only when it is safe to return the visitor to after sign-in.
 *
 * @param candidate - A requested return path, if any.
 * @returns The path when it is inside this app and not an `/api/` route, otherwise null.
 */
export const safeReturnPath = (candidate: string | undefined): string | null => {
  return candidate !== undefined && isInternalPath(candidate) && !candidate.startsWith('/api/') ? candidate : null
}

/**
 * Starts the sign-on flow for this browser.
 *
 * Fails closed with a coded 503 before any cookie is written when the sign-on addresses are not
 * configured. Otherwise writes the PKCE verifier and `state` cookies, plus the return-path cookie
 * when one is given, and returns the authorize query.
 *
 * @param event - The request being answered.
 * @param returnTo - A path already checked with `safeReturnPath`, or null to remember none.
 * @returns The provider address and the authorize query.
 * @throws H3Error 503 carrying `data.code = SSO_NOT_CONFIGURED` when an address is missing.
 */
export const startSignonFlow = (event: H3Event, returnTo: string | null): SignonFlow => {
  const config = useRuntimeConfig()
  const ssoPublicUrl = requireSsoSetting(config.public.ssoPublicUrl)
  const redirectUri = requireSsoSetting(config.ssoCallbackUrl)

  const codeVerifier = randomBytes(32).toString('base64url')
  const codeChallenge = createHash('sha256')
    .update(codeVerifier)
    .digest('base64url')
  setCookie(event, SessionCookie.PkceVerifier, codeVerifier, cookieOptions(SSO_FLOW_COOKIE_MAX_AGE))

  const state = randomBytes(16).toString('hex')
  setCookie(event, SessionCookie.OauthState, state, cookieOptions(SSO_FLOW_COOKIE_MAX_AGE))

  if (returnTo !== null) {
    setCookie(event, SessionCookie.PostLoginRedirect, returnTo, cookieOptions(SSO_FLOW_COOKIE_MAX_AGE))
  }

  const params = new URLSearchParams({
    client_id: config.ssoClientId,
    response_type: 'code',
    redirect_uri: redirectUri,
    scope: SIGNON_SCOPE,
    code_challenge: codeChallenge,
    code_challenge_method: 'S256',
    state,
  })
  return { ssoPublicUrl, params }
}
