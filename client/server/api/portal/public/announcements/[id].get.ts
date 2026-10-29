/**
 * GET /api/portal/public/announcements/:id
 *
 * One published announcement, body included, passed through as the backend sends it. The backend's
 * public detail route answers an unknown id and a draft the same way, so the page cannot be used
 * to find unpublished announcements. The language reaches the backend as `X-Language` (stamped by
 * `internalApiCall`).
 *
 * @module server/api/portal/public/announcements/[id].get
 */

import type { PublishedAnnouncement } from '~/types/publishedannouncement'

/** Handler: the backend's published announcement for the id in the route. */
export default defineEventHandler(async (event): Promise<PublishedAnnouncement> => {
  const id = requireRouteParam(event, 'id')

  return await internalApiCall<PublishedAnnouncement>(event, `/public/announcements/${id}`)
})
