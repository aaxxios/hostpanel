/**
 * PUT /api/portal/client/domains/:id/lock
 * Toggles registrar lock status for a domain via the C# backend.
 *
 * Body: { enabled: boolean }
 */
export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  const body = await readBody(event)
  return await internalApiCall<Record<string, unknown>>(event, `/me/domains/${id}/lock`, {
    method: 'PUT',
    body,
  })
})
