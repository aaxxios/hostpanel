/**
 * GET /api/portal/client/tickets/:id
 * Returns the full thread of a single support ticket from the C# backend.
 */
export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  return await internalApiCall<Record<string, unknown>>(event, `/me/tickets/${id}`)
})
