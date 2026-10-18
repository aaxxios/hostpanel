/**
 * Route-parameter guard for the Nitro routes.
 *
 * A `[id]` route file cannot be reached without its segment, so a missing parameter is a
 * programmer fault, never something a visitor causes -- the answer is therefore a coded refusal
 * and no sentence. The browser renders the generic title for it; writing "ID is required" here
 * would be a BFF-authored sentence in one language, which the localization rule forbids.
 *
 * @module server/utils/requireRouteParam
 */

import type { H3Event } from 'h3'

/** The `data.code` of the 400 a missing route parameter answers with. */
export const BAD_ROUTE_PARAM = 'BAD_ROUTE_PARAM'

/**
 * Reads a route parameter and refuses the request when it is absent or empty.
 *
 * @param event - H3 event of the incoming request.
 * @param name - The parameter's name as declared in the file name, e.g. `id` for `[id].get.ts`.
 * @returns The parameter's (decoded) value, never empty.
 * @throws H3Error 400 carrying `data.code = BAD_ROUTE_PARAM` and no `statusMessage`.
 */
export const requireRouteParam = (event: H3Event, name: string): string => {
  const value = getRouterParam(event, name)
  if (!value) {
    throw createError({ statusCode: 400, data: { code: BAD_ROUTE_PARAM } })
  }
  return value
}
