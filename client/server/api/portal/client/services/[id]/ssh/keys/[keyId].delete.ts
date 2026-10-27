/**
 * DELETE /api/portal/client/services/:id/ssh/keys/:keyId
 * Revokes one SSH key of one of the caller's services. Both ids are encoded into the backend path,
 * whose route constraints (`int`, `guid`) refuse anything else.
 *
 * @module server/api/portal/client/services/[id]/ssh/keys/[keyId].delete
 */
export default defineEventHandler(async (event) => {
  const id = encodeURIComponent(requireRouteParam(event, 'id'))
  const keyId = encodeURIComponent(requireRouteParam(event, 'keyId'))
  await internalApiCall<unknown>(event, `/me/services/${id}/ssh/keys/${keyId}`, { method: 'DELETE' })
  setResponseStatus(event, 204)
  return null
})
