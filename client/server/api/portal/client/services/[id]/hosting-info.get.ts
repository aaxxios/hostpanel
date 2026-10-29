/**
 * GET /api/portal/client/services/:id/hosting-info
 * Returns the nameservers and SSL certificate of the service's domain.
 *
 * A pass-through: the backend owns the whole read. It checks the service is the caller's, takes the
 * domain from the service row and probes it behind its own network guard, so nothing here resolves
 * a name or opens a connection on a visitor's behalf.
 *
 * @module server/api/portal/client/services/[id]/hosting-info.get
 */

import type { HostingInfo } from '~/types/hostinginfo'

export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  return await internalApiCall<HostingInfo>(event, `/me/services/${id}/hosting-info`)
})
