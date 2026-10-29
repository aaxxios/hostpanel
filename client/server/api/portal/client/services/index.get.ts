/**
 * GET /api/portal/client/services
 * Returns the authenticated client's services exactly as the backend sends them.
 */
import type { ClientService } from '~/types/clientservice'

export default defineEventHandler((event): Promise<ClientService[]> =>
  internalApiCall<ClientService[]>(event, '/me/services'))
