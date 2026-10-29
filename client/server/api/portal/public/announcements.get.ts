/**
 * GET /api/portal/public/announcements
 *
 * The dashboard's "Recent News" card: the first page of ten *published* announcements, passed
 * through as the backend sends them. It calls the client-facing `/announcements/published`,
 * whose projection has no `isPublished` flag and never contains a draft; the card formats
 * `publishedAt` itself.
 *
 * @module server/api/portal/public/announcements.get
 */

import type { PaginatedList } from '~/types/paginatedlist'
import type { PublishedAnnouncement } from '~/types/publishedannouncement'

export default defineEventHandler(async (event): Promise<PaginatedList<PublishedAnnouncement>> =>
  await internalApiCall<PaginatedList<PublishedAnnouncement>>(event, '/announcements/published?page=1&pageSize=10')
)
