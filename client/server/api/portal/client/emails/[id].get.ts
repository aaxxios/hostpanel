/**
 * GET /api/portal/client/emails/:id
 * Returns the full message of a single email: the backend `MyEmailLogDetailDto`, passed through
 * as sent.
 *
 * @module server/api/portal/client/emails/[id].get
 */

import type { ClientEmail } from '~/types/clientemail'

export default defineEventHandler(async (event): Promise<ClientEmail> => {
  const id = requireRouteParam(event, 'id')

  return await internalApiCall<ClientEmail>(event, `/me/emails/${id}`)
})
