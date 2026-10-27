/**
 * GET /api/portal/client/services/:id/ssh
 * SSH access summary of one of the caller's services. Server, account and port are resolved by
 * the backend from the service; nothing but the service id is forwarded.
 *
 * @module server/api/portal/client/services/[id]/ssh/index.get
 */
export default defineEventHandler(async (event) => {
  const id = encodeURIComponent(requireRouteParam(event, 'id'))
  return await internalApiCall<Record<string, unknown>>(event, `/me/services/${id}/ssh`)
})
