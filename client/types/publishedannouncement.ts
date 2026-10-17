/**
 * One published announcement exactly as the backend returns it.
 *
 * @module types/publishedannouncement
 */

/** One published announcement exactly as the backend returns it. */
export interface PublishedAnnouncement {
  /** Announcement primary key. */
  id: number
  /** Announcement headline. */
  title: string
  /** Body content, rendered through `UiRichText` by the detail page. */
  content: string
  /** ISO-8601 UTC timestamp the announcement was published. */
  publishedAt: string
}
