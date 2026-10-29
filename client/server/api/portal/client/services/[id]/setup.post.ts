/**
 * POST /api/portal/client/services/:id/setup
 * Finalizes service setup (domain, username, password) via the C# backend.
 */

import { HttpMethod } from '~/types/httpmethod'

export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  const body = await readBody(event)
  return await internalApiCall<Record<string, unknown>>(event, `/me/services/${id}/setup`, {
    method: HttpMethod.Post,
    body,
  })
})
