/**
 * GET /api/portal/public/partners
 *
 * The active partners, in display order. The language reaches the backend as `X-Language`
 * (stamped by `internalApiCall`).
 *
 * @module server/api/portal/public/partners.get
 */

import type { Partner } from '~/types/partner'

/** Handler: the backend's active partners, passed through as sent. */
export default defineEventHandler(async (event): Promise<Partner[]> =>
  await internalApiCall<Partner[]>(event, '/public/partners'))
