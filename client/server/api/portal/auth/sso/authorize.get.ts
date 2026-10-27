/**
 * GET /api/portal/auth/sso/authorize
 *
 * Generates a PKCE pair, stores code_verifier in a short-lived httpOnly cookie,
 * then redirects the browser to the SSO authorization endpoint.
 *
 * An `authuser` query (the account a visitor arrived for, as the products of a shared sign-on add
 * to their links) is passed on as `login_hint`, so the identity provider picks or pre-fills that
 * account instead of whichever one it signed in last.
 *
 * @module server/api/portal/auth/sso/authorize.get
 */

import { randomBytes, createHash } from 'node:crypto'
import { LOGIN_HINT_QUERY } from '~/constants/clientRoutes'
import { SessionCookie } from '~/constants/sessionCookie'
import { SSO_FLOW_COOKIE_MAX_AGE } from '~/server/utils/auth-cookie'
import { requireSsoSetting } from '~/server/utils/requireSsoSetting'

/** The longest `login_hint` passed on; the longest valid e-mail address is 254 characters. */
const MAX_LOGIN_HINT_LENGTH = 254

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig()
  // Fail closed before any cookie is set: an unconfigured SSO has nowhere to send the browser.
  const ssoPublicUrl = requireSsoSetting(config.public.ssoPublicUrl)
  const redirectUri = requireSsoSetting(config.ssoCallbackUrl)

  // Generate PKCE
  const codeVerifier = randomBytes(32).toString('base64url')
  const codeChallenge = createHash('sha256')
    .update(codeVerifier)
    .digest('base64url')

  // Store verifier server-side in an httpOnly cookie (5 min TTL)
  setCookie(event, SessionCookie.PkceVerifier, codeVerifier, {
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'lax',
    maxAge: SSO_FLOW_COOKIE_MAX_AGE,
    path: '/',
  })

  // Generate and store state for CSRF protection
  const state = randomBytes(16).toString('hex')
  setCookie(event, SessionCookie.OauthState, state, {
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'lax',
    maxAge: SSO_FLOW_COOKIE_MAX_AGE,
    path: '/',
  })

  const query = getQuery(event)
  const params = new URLSearchParams({
    client_id: config.ssoClientId,
    response_type: 'code',
    redirect_uri: redirectUri,
    scope: 'openid profile email offline_access',
    code_challenge: codeChallenge,
    code_challenge_method: 'S256',
    state,
  })

  // If the user explicitly wants to switch accounts, pass prompt=select_account
  if (query.prompt) {
    params.set('prompt', query.prompt as string)
  }

  // The account the visitor arrived for. Capped: an address is never longer, and the value travels on in a URL.
  const loginHint = query[LOGIN_HINT_QUERY]
  if (typeof loginHint === 'string' && loginHint.trim() && loginHint.length <= MAX_LOGIN_HINT_LENGTH) {
    params.set('login_hint', loginHint.trim())
  }

  // Save current page so callback can return here instead of always going to dashboard
  let returnTo = (query.returnTo as string | undefined) || ''
  if (!returnTo) {
    const referer = getHeader(event, 'referer') || ''
    if (referer) {
      try { returnTo = new URL(referer).pathname } catch { /* ignore */ }
    }
  }
  if (returnTo && returnTo.startsWith('/') && !returnTo.startsWith('/api/')) {
    setCookie(event, SessionCookie.PostLoginRedirect, returnTo, {
      httpOnly: true,
      secure: process.env.NODE_ENV === 'production',
      sameSite: 'lax',
      maxAge: SSO_FLOW_COOKIE_MAX_AGE,
      path: '/',
    })
  }

  const authorizeUrl = `${ssoPublicUrl}/connect/authorize?${params}`
  return sendRedirect(event, authorizeUrl, 302)
})
