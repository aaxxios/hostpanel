/**
 * Server middleware: CSRF
 *
 * Refuses a state-changing request that does not carry
 * `X-Requested-With: XMLHttpRequest`.
 *
 * The session lives in a cookie, and a cookie is attached by the browser to
 * requests this app did not make. SameSite=Lax stops the obvious version of that
 * for POST, but it is one flag on one cookie, decided by the browser: it does not
 * apply to every client, it has been relaxed before, and it leaves nothing to check
 * on this side. A custom header cannot be set cross-site without a CORS preflight
 * this app never approves, so its presence is evidence the request came from here.
 *
 * The backend's sign-on package runs the same check in `RequireCustomHeaderMiddleware`,
 * with the same refusal text. Without it `POST /api/portal/auth/logout` would be
 * accepted from any page on the internet, signing the visitor out with nothing on this
 * side able to tell that it had happened.
 *
 * Bearer callers are exempt: a bearer token has to be attached deliberately, which
 * a cross-site page cannot do, and the server-to-server callers that use one are
 * not browsers.
 *
 * @module server/middleware/csrf
 */

import { HttpMethod } from '~/types/httpmethod'

/** Methods that never change state, and so are never checked. */
const SAFE_METHODS: ReadonlySet<string> = new Set([HttpMethod.Get, HttpMethod.Head, HttpMethod.Options])

/** Refuses a state-changing `/api/` request that lacks the custom header. */
export default defineEventHandler((event) => {
  if (SAFE_METHODS.has(event.method)) {
    return
  }

  // Only this app's own API. Everything else Nitro serves is a page render.
  if (!(event.path ?? '').startsWith('/api/')) {
    return
  }

  if (getHeader(event, 'authorization')?.startsWith('Bearer ')) {
    return
  }

  if (getHeader(event, 'x-requested-with') === 'XMLHttpRequest') {
    return
  }

  throw createError({
    statusCode: 403,
    message:
      'This endpoint changes state and requires the header '
      + 'X-Requested-With: XMLHttpRequest.',
  })
})
