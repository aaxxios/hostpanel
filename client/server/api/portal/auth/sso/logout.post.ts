/**
 * POST /api/portal/auth/sso/logout
 *
 * Ends the server-side session (the stored tokens and the cookies), then redirects to the SSO
 * end-session endpoint.
 *
 * @module server/api/portal/auth/sso/logout.post
 */

import { endSession } from '~/server/utils/portal-session'
import { requireSsoSetting } from '~/server/utils/requireSsoSetting'

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig()
  // Fail closed before the session is cleared: without an SSO address there is no end-session page to go to.
  const ssoPublicUrl = requireSsoSetting(config.public.ssoPublicUrl)

  await endSession(event)

  const baseUrl = config.public.baseUrl || getRequestURL(event).origin
  const endsessionUrl = `${ssoPublicUrl}/connect/endsession?post_logout_redirect_uri=${encodeURIComponent(baseUrl)}`
  return sendRedirect(event, endsessionUrl, 302)
})
