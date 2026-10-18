/**
 * PUT /api/portal/client/domains/:id/autorenew
 * Toggles auto-renew for a domain via the C# backend.
 *
 * Body: { enabled: boolean }
 */
export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  const body = await readBody(event)
  return await internalApiCall<Record<string, unknown>>(event, `/me/domains/${id}/auto-renew`, {
    method: 'PUT',
    body,
  })
})
