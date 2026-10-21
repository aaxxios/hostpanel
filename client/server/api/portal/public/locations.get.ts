/**
 * GET /api/portal/public/locations
 *
 * The active server locations, in display order. The language reaches the backend as
 * `X-Language` (stamped by `internalApiCall`).
 *
 * @module server/api/portal/public/locations.get
 */

import type { ServerLocation } from '~/types/serverlocation'

/** Handler: the backend's active server locations, passed through as sent. */
export default defineEventHandler(async (event): Promise<ServerLocation[]> =>
  await internalApiCall<ServerLocation[]>(event, '/public/locations'))
