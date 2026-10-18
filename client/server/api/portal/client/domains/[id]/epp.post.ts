/**
 * POST /api/portal/client/domains/:id/epp
 * Requests the EPP (auth) code for a domain via the C# backend.
 */
export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  return await internalApiCall<Record<string, unknown>>(event, `/me/domains/${id}/epp`, {
    method: 'POST',
  })
})
