/**
 * The backend's paging envelope around a page of announcements.
 *
 * @module types/publishedannouncementpage
 */

import type { PublishedAnnouncement } from '~/types/publishedannouncement'

/** The backend's paging envelope around a page of announcements. */
export interface PublishedAnnouncementPage {
  /** The announcements on this page, newest first. */
  items: PublishedAnnouncement[]
  /** Total published announcements across all pages. */
  totalCount: number
  /** The 1-based page number this envelope describes. */
  page: number
  /** How many items a full page holds. */
  pageSize: number
}
