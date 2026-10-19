/**
 * GET /api/portal/auth/sso/authorize
 *
 * Generates a PKCE pair, stores code_verifier in a short-lived httpOnly cookie,
 * then redirects the browser to the SSO authorization endpoint.
 *
 * @module server/api/portal/auth/sso/authorize.get
 */

import { randomBytes, createHash } from 'node:crypto'
import { SessionCookie } from '~/constants/sessionCookie'
import { SSO_FLOW_COOKIE_MAX_AGE } from '~/server/utils/auth-cookie'
import { requireSsoSetting } from '~/server/utils/requireSsoSetting'

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
