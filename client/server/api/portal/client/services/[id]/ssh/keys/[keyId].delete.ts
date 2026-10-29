/**
 * DELETE /api/portal/client/services/:id/ssh/keys/:keyId
 * Revokes one SSH key of one of the caller's services. Both ids are validated (an integer and a GUID)
 * and encoded into the backend path by `requireRouteParam`.
 *
 * @module server/api/portal/client/services/[id]/ssh/keys/[keyId].delete
 */

import { HttpMethod } from '~/types/httpmethod'
import { RouteParamKind } from '~/types/routeparamkind'

export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')
  const keyId = requireRouteParam(event, 'keyId', RouteParamKind.Guid)
  await internalApiCall<unknown>(event, `/me/services/${id}/ssh/keys/${keyId}`, { method: HttpMethod.Delete })
  setResponseStatus(event, 204)
  return null
})
