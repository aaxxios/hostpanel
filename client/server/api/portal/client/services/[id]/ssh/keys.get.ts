/**
 * GET /api/portal/client/services/:id/ssh/keys
 * Lists the SSH keys of one of the caller's services.
 *
 * @module server/api/portal/client/services/[id]/ssh/keys.get
 */
export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')
  return await internalApiCall<unknown[]>(event, `/me/services/${id}/ssh/keys`)
})
