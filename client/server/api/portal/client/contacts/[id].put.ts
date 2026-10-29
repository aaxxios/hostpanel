/**
 * PUT /api/portal/client/contacts/:id
 * Updates an existing contact via the C# backend.
 */

import { HttpMethod } from '~/types/httpmethod'

export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  const body = await readBody(event)
  return await internalApiCall<Record<string, unknown>>(event, `/clients/me/contacts/${id}`, {
    method: HttpMethod.Put,
    body,
  })
})
