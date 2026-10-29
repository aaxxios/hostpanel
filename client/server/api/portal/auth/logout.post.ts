/**
 * POST /api/portal/auth/logout
 *
 * Called by the shared header widget when the user clicks Sign out.
 * 1. Revokes the refresh token on the hostpanel backend.
 * 2. Ends the server-side session: deletes the stored tokens and clears the cookies.
 *
 * The access token never leaves this server. Ending the sign-on provider's own session is the
 * widget's step, at the provider's end-session endpoint, and the provider then notifies this
 * application through its backchannel logout.
 *
 * @module server/api/portal/auth/logout.post
 */
import { endSession, readSession } from '~/server/utils/portal-session'
import { HttpMethod } from '~/types/httpmethod'

export default defineEventHandler(async (event) => {
  const current = await readSession(event)
  const refreshToken = current?.session.refreshToken

  // 1. Revoke refresh token on hostpanel backend
  try {
    // Revocation returns no body, and the surrounding `catch` already treats any failure as
    // "clear the cookies anyway". Anonymous: the refresh token (sent as a cookie) is the credential.
    await internalApiCall<string>(event, '/auth/logout', {
      method: HttpMethod.Post,
      anonymous: true,
      headers: refreshToken ? { Cookie: `refreshToken=${refreshToken}` } : {},
    })
  } catch {
    // Ignore: clear cookies regardless
  }

  // 2. End the session: the stored tokens and the cookies
  await endSession(event)
  return { ok: true }
})
