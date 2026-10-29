/**
 * POST /api/portal/client/tickets/:id/reply
 * Adds a client reply to an existing support ticket via the C# backend.
 *
 * Body: { message }
 */

import { HttpMethod } from '~/types/httpmethod'

export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  const body = await readBody(event)
  return await internalApiCall<Record<string, unknown>>(event, `/me/tickets/${id}/reply`, {
    method: HttpMethod.Post,
    body,
  })
})
