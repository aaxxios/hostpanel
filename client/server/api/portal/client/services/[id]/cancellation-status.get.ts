/**
 * GET /api/portal/client/services/:id/cancellation-status
 * Checks if there is a pending cancellation request for this service via the C# backend.
 */
export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  return await internalApiCall<Record<string, unknown>>(event, `/me/services/${id}/cancellation-status`)
})
