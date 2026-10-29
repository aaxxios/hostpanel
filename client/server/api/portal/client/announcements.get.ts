/**
 * GET /api/portal/client/announcements
 *
 * The signed-in announcements list: the backend's page of `PublishedAnnouncementDto` rows, passed
 * through as sent. The page formats `publishedAt` for the reader's locale and derives its month
 * tabs from it, so the raw timestamp is what it needs.
 *
 * The backend pages with `page`/`pageSize`; both are forwarded, defaulting to page one of 50.
 *
 * @module server/api/portal/client/announcements.get
 */
import type { PaginatedList } from '~/types/paginatedlist'
import type { PublishedAnnouncement } from '~/types/publishedannouncement'

/** Forwards the requested page of published announcements and answers with the backend's page. */
export default defineEventHandler(async (event): Promise<PaginatedList<PublishedAnnouncement>> => {
  const query = getQuery(event)
  const page = Number(query.page ?? 1)
  const pageSize = Number(query.pageSize ?? 50)

  return await internalApiCall<PaginatedList<PublishedAnnouncement>>(
    event,
    `/announcements/published?page=${page}&pageSize=${pageSize}`
  )
})
