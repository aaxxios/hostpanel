/**
 * PUT /api/portal/client/payment-methods/:id
 * Updates a stored payment method via the C# backend.
 */

import { HttpMethod } from '~/types/httpmethod'
import { RouteParamKind } from '~/types/routeparamkind'

export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id', RouteParamKind.Identifier)

  const body = await readBody(event)
  return await internalApiCall<Record<string, unknown>>(event, `/me/payment-methods/${id}`, {
    method: HttpMethod.Put,
    body,
  })
})
