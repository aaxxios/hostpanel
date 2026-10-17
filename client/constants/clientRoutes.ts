/**
 * The client area's well-known paths, in one place so the route middleware, the session-expiry
 * plugin and the sign-in page agree on what the login page is and which pages need no session.
 *
 * @module constants/clientRoutes
 */

/** The sign-in page. */
export const CLIENT_LOGIN_PATH = '/client/login'

/** Where a signed-in customer lands when nothing more specific was asked for. */
export const CLIENT_DASHBOARD_PATH = '/client/dashboard'

/** The query parameter that carries the page to return to after signing in. */
export const CLIENT_REDIRECT_QUERY = 'redirect'

/** The client pages that need no session; a path equal to one of these, or below it, is public. */
export const CLIENT_PUBLIC_ROUTES: readonly string[] = [
  CLIENT_LOGIN_PATH,
  '/client/register',
  '/client/forgot-password',
  '/client/reset-password',
  '/client/confirm-email'
]
