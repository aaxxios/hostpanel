/**
 * POST /api/portal/client/services/:id/setup
 * Finalizes service setup (domain, username, password) via the C# backend.
 */
export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  const body = await readBody(event)
  return await internalApiCall<Record<string, unknown>>(event, `/me/services/${id}/setup`, {
    method: 'POST',
    body,
  })
})
