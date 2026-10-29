/**
 * GET /api/portal/client/services/:id
 * Returns one of the authenticated client's services exactly as the backend sends it.
 * The backend only has a list endpoint, so the list is read and the one service picked from it.
 */
import { PortalErrorCode } from '~/constants/portalErrorCode'
import type { ClientService } from '~/types/clientservice'

export default defineEventHandler(async (event): Promise<ClientService> => {
  const id = requireRouteParam(event, 'id')

  const services = await internalApiCall<ClientService[]>(event, '/me/services')
  const service = services.find((svc) => String(svc.id) === id)

  if (!service) {
    // The backend has no single-service read, so this refusal is the BFF's own: a code, no sentence.
    throw createError({ statusCode: 404, data: { code: PortalErrorCode.ServiceNotFound } })
  }

  return service
})
