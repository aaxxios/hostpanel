/**
 * The trimmed announcement the dashboard card renders.
 *
 * @module types/announcementsummary
 */

/**
 * The trimmed announcement the dashboard card renders.
 *
 * Narrower than {@link Announcement} on purpose: the card shows a headline and a date and has
 * no room for a body, so the public proxy does not carry one.
 */
export interface AnnouncementSummary {
  /** Announcement primary key, used as the list key. */
  id: number
  /** Announcement headline. */
  title: string
  /** Publication date, already formatted as `YYYY-MM-DD` for display. */
  date: string
}
