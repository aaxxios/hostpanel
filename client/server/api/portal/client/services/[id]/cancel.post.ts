/**
 * POST /api/portal/client/services/:id/cancel
 * Submits a cancellation request for a hosting service via the C# backend.
 */

import { HttpMethod } from '~/types/httpmethod'

export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  const body = await readBody(event)
  return await internalApiCall<Record<string, unknown>>(event, `/me/services/${id}/cancel`, {
    method: HttpMethod.Post,
    body,
  })
})
