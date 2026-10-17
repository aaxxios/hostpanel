/**
 * POST /api/portal/auth/login
 *
 * Signs in against the C# API (local-auth mode) and stores the session in httpOnly cookies. The
 * call is anonymous -- no session exists yet -- but still goes through `internalApiCall`, so the
 * reader's `X-Language` reaches the backend and a refusal arrives as the backend's own
 * `detail` / `code` rather than a raw upstream error.
 *
 * @module server/api/portal/auth/login.post
 */

/** What the backend answers a sign-in with. */
interface LoginResult {
  /** Access token; absent when a second factor is still required. */
  accessToken?: string
  /** Lifetime of the access token, in seconds. */
  expiresIn?: number
  /** True when the account has two-factor on and the sign-in is not complete yet. */
  twoFactorRequired?: boolean
  /** Short-lived token that the `2fa-login` call presents together with the code. */
  pendingToken?: string
}

/**
 * Signs in and sets the session cookies, or passes a two-factor challenge through.
 */
export default defineEventHandler(async (event) => {
  const body = await readBody(event)

  const response = await internalApiCall<LoginResult>(
    event,
    '/auth/login',
    { method: 'POST', body, anonymous: true }
  )

  // 2FA required — pass through without setting cookies
  if (response.twoFactorRequired) {
    return response
  }

  // Set auth cookies
  if (response.accessToken) {
    setCookie(event, 'auth_token', response.accessToken, {
      httpOnly: true,
      secure: process.env.NODE_ENV === 'production',
      sameSite: 'lax',
      maxAge: response.expiresIn ?? 900,
      path: '/',
    })
    setCookie(event, 'authed', '1', {
      httpOnly: false,
      secure: process.env.NODE_ENV === 'production',
      sameSite: 'lax',
      maxAge: 60 * 60 * 24 * 7,
      path: '/',
    })
  }

  return { success: true }
})
