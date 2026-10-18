/**
 * GET /api/portal/client/domains/:id/nameservers
 * Returns the nameservers currently set on a domain via the C# backend.
 */
export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  return await internalApiCall<Record<string, unknown>>(event, `/me/domains/${id}/nameservers`)
})
