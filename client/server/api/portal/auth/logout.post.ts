/**
 * POST /api/portal/auth/logout
 *
 * Called by the shared header widget when the user clicks Sign out.
 * 1. Revokes the refresh token on the hostpanel backend.
 * 2. Fires backchannel logout to all platform services via the main portal, when one is
 *    configured (`mainApiUrl`); with none configured the call is skipped.
 * 3. Ends the server-side session: deletes the stored tokens and clears the cookies.
 *
 * @module server/api/portal/auth/logout.post
 */
import { readTokenPayload } from '~/server/utils/auth-cookie'
import { endSession, readSession } from '~/server/utils/portal-session'

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig()
  const current = await readSession(event)
  const refreshToken = current?.session.refreshToken
  const authToken = current?.session.accessToken

  // 1. Revoke refresh token on hostpanel backend
  try {
    // Revocation returns no body, and the surrounding `catch` already treats any failure as
    // "clear the cookies anyway". Anonymous: the refresh token (sent as a cookie) is the credential.
    await internalApiCall<string>(event, '/auth/logout', {
      method: 'POST',
      anonymous: true,
      headers: refreshToken ? { Cookie: `refreshToken=${refreshToken}` } : {},
    })
  } catch {
    // Ignore — clear cookies regardless
  }

  // 2. Fire backchannel logout to all platform services via the main portal.
  // The widget goes to SSO endsession after this returns, but backchannel
  // calls are fire-and-forget so we don't block the response on them.
  if (authToken) {
    try {
      const claims = readTokenPayload(authToken) ?? {}
      const email = String(claims['email'] || claims['sub'] || '')
      // No fallback host: a self-hosted install without a main portal has nothing to notify.
      const mainApiUrl = (config.mainApiUrl as string) || ''
      // The post-logout return address is this portal's own configured public URL.
      const siteUrl = (config.public.baseUrl as string) || ''
      if (email && mainApiUrl) {
        fetch(`${mainApiUrl}/api/portal/auth/sso/logout-redirect?redirect_uri=${encodeURIComponent(siteUrl)}`, {
          headers: { Cookie: `auth_token=${authToken}` },
          redirect: 'manual',
          signal: AbortSignal.timeout(3000),
        }).catch(() => {})
      }
    } catch { /* ignore */ }
  }

  // 3. End the session: the stored tokens and the cookies
  await endSession(event)
  return { ok: true }
})
