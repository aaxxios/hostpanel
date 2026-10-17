/**
 * GET /api/portal/client/domains/:id
 * Returns full information for a single client domain from the C# backend.
 */
export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  return await internalApiCall<Record<string, unknown>>(event, `/me/domains/${id}`)
})
