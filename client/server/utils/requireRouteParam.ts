/**
 * Route-parameter guard for the Nitro routes.
 *
 * A `[id]` route file cannot be reached without its segment, but a visitor chooses what the
 * segment holds, and most routes splice it into the path of a backend call. So the parameter is
 * checked against the shape the backend route accepts (an integer, a GUID or an opaque id) and
 * only then handed on, percent-encoded. Anything else is a coded 400 and no sentence: the BFF
 * authors none, the browser renders the generic title, and writing "ID is invalid" here would be
 * a BFF-authored sentence in one language, which the localization rule forbids.
 *
 * @module server/utils/requireRouteParam
 */

import type { H3Event } from 'h3'
import { RouteParamKind } from '~/types/routeparamkind'

/** The `data.code` of the 400 a missing or malformed route parameter answers with. */
export const BAD_ROUTE_PARAM = 'BAD_ROUTE_PARAM'

/** The largest value of a 32-bit signed integer key. */
const MAX_INT_KEY = 2147483647

/** The shape of each parameter kind, other than the integer range which is checked numerically. */
const SHAPES: Record<RouteParamKind, RegExp> = {
  [RouteParamKind.Int]: /^[1-9]\d{0,9}$/,
  [RouteParamKind.Guid]: /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i,
  [RouteParamKind.Identifier]: /^[A-Za-z0-9_-]{1,128}$/
}

/**
 * Whether a value has the shape of the requested kind.
 *
 * @param value - The decoded route parameter.
 * @param kind - The shape the route accepts.
 * @returns True when the value is well formed for the kind.
 */
const hasShape = (value: string, kind: RouteParamKind): boolean => {
  if (!SHAPES[kind].test(value)) {
    return false
  }
  return kind !== RouteParamKind.Int || Number(value) <= MAX_INT_KEY
}

/**
 * Reads a route parameter, refuses the request when it is absent or malformed, and returns it
 * encoded for use inside a backend path.
 *
 * @param event - H3 event of the incoming request.
 * @param name - The parameter's name as declared in the file name, e.g. `id` for `[id].get.ts`.
 * @param kind - The shape the route accepts; a positive integer when omitted.
 * @returns The validated parameter, percent-encoded (a no-op for every accepted shape, kept so
 *   no later change to a shape can reintroduce raw interpolation).
 * @throws H3Error 400 carrying `data.code = BAD_ROUTE_PARAM` and no `statusMessage`.
 */
export const requireRouteParam = (event: H3Event, name: string, kind: RouteParamKind = RouteParamKind.Int): string => {
  const value = getRouterParam(event, name)
  if (!value || !hasShape(value, kind)) {
    throw createError({ statusCode: 400, data: { code: BAD_ROUTE_PARAM } })
  }
  return encodeURIComponent(value)
}
