/**
 * GET /api/portal/auth/sso/switch?sub=XXX&redirect=/some/path
 *
 * Switches the active SSO session to a remembered account.
 * Generates a PKCE pair and redirects to sso.innovayse.local/switch-init
 * which auto-submits to the SSO switch endpoint, then comes back
 * via the normal OIDC callback, which starts a new session.
 *
 * @module server/api/portal/auth/sso/switch.get
 */

import { randomBytes, createHash } from 'node:crypto'
import { CLIENT_AREA_PATH, SSO_AUTHORIZE_PATH } from '~/constants/clientRoutes'
import { SessionCookie } from '~/constants/sessionCookie'
import { SSO_FLOW_COOKIE_MAX_AGE } from '~/server/utils/auth-cookie'
import { requireSsoSetting } from '~/server/utils/requireSsoSetting'

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig()
  const query = getQuery(event)
  const sub = query.sub as string | undefined
  const redirectTo = (query.redirect as string | undefined) || CLIENT_AREA_PATH

  if (!sub) {
    return sendRedirect(event, SSO_AUTHORIZE_PATH, 302)
  }

  // Fail closed before any cookie is set: an unconfigured SSO has nowhere to send the browser.
  const ssoPublicUrl = requireSsoSetting(config.public.ssoPublicUrl)
  const redirectUri = requireSsoSetting(config.ssoCallbackUrl)

  // Generate PKCE
  const codeVerifier = randomBytes(32).toString('base64url')
  const codeChallenge = createHash('sha256')
    .update(codeVerifier)
    .digest('base64url')

  setCookie(event, SessionCookie.PkceVerifier, codeVerifier, {
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'lax',
    maxAge: SSO_FLOW_COOKIE_MAX_AGE,
    path: '/',
  })

  const state = randomBytes(16).toString('hex')
  setCookie(event, SessionCookie.OauthState, state, {
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'lax',
    maxAge: SSO_FLOW_COOKIE_MAX_AGE,
    path: '/',
  })

  setCookie(event, SessionCookie.PostLoginRedirect, redirectTo, {
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'lax',
    maxAge: SSO_FLOW_COOKIE_MAX_AGE,
    path: '/',
  })

  const oidcParams = new URLSearchParams({
    client_id: config.ssoClientId,
    response_type: 'code',
    redirect_uri: redirectUri,
    scope: 'openid profile email offline_access',
    code_challenge: codeChallenge,
    code_challenge_method: 'S256',
    state,
  })
  const oidcReturnUrl = `/connect/authorize?${oidcParams}`

  // Redirect to the SSO's switch-init page — runs same-origin so the sso_remembered cookie is sent
  const switchInitUrl = `${ssoPublicUrl}/switch-init?sub=${encodeURIComponent(sub)}&returnUrl=${encodeURIComponent(oidcReturnUrl)}`
  return sendRedirect(event, switchInitUrl, 302)
})
