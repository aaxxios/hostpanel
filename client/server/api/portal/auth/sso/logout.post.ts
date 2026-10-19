/**
 * POST /api/portal/auth/sso/logout
 *
 * Clears all auth cookies, then redirects to SSO end-session endpoint.
 */

import { SessionCookie } from '~/constants/sessionCookie'
import { requireSsoSetting } from '~/server/utils/requireSsoSetting'

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig()
  // Fail closed before the session is cleared: without an SSO address there is no end-session page to go to.
  const ssoPublicUrl = requireSsoSetting(config.public.ssoPublicUrl)

  deleteCookie(event, SessionCookie.AuthToken, { path: '/' })
  deleteCookie(event, SessionCookie.RefreshToken, { path: '/' })
  deleteCookie(event, SessionCookie.Authed, { path: '/' })

  const baseUrl = config.public.baseUrl || getRequestURL(event).origin
  const endsessionUrl = `${ssoPublicUrl}/connect/endsession?post_logout_redirect_uri=${encodeURIComponent(baseUrl)}`
  return sendRedirect(event, endsessionUrl, 302)
})
