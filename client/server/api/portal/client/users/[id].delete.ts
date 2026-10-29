/**
 * DELETE /api/portal/client/users/:id
 * Removes a user's access to the authenticated client account via the C# backend.
 */

import { HttpMethod } from '~/types/httpmethod'
import { RouteParamKind } from '~/types/routeparamkind'

export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id', RouteParamKind.Identifier)

  return await internalApiCall<Record<string, unknown>>(event, `/me/users/${id}`, {
    method: HttpMethod.Delete,
  })
})
