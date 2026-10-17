/**
 * GET /api/portal/client/emails/:id
 * Returns the full message body of a single email from the C# backend.
 */
export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  return await internalApiCall<Record<string, unknown>>(event, `/me/emails/${id}`)
})
