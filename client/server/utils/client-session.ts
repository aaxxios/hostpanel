/**
 * Server-side session utility — used in all protected /api/portal/client/* routes
 *
 * Reads the `client_session` httpOnly cookie set by `POST /api/portal/auth/login`,
 * parses it, and returns the typed session object.
 *
 * Throws a 401 if the cookie is missing or malformed, so callers don't need
 * to add their own auth checks — just call `requireClientSession(event)` at the top.
 *
 * Usage:
 * ```ts
 * export default defineEventHandler(async (event) => {
 *   const session = requireClientSession(event)
 *   // session.id   → legacy client ID
 *   // session.email
 *   // session.name
 * })
 * ```
 *
 * @module server/utils/client-session
 */

import { SessionCookie } from '~/constants/sessionCookie'
import type { ClientSession } from '~/types/clientsession'

/**
 * Read and validate the `client_session` session cookie from the incoming request.
 *
 * @param event - H3 event from `defineEventHandler`
 * @returns Parsed `ClientSession`
 * @throws H3Error 401 if the cookie is absent, unparseable, or missing `id`
 */
export const requireClientSession = (event: Parameters<typeof getCookie>[0]): ClientSession => {
  const raw = getCookie(event, SessionCookie.ClientSession)

  if (!raw) {
    throw createError({ statusCode: 401 })
  }

  try {
    const session = JSON.parse(raw) as ClientSession
    if (!session?.id) {
      throw new Error('Invalid session payload')
    }
    return session
  } catch {
    throw createError({ statusCode: 401 })
  }
}
