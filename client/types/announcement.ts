/**
 * The announcement a portal page renders.
 *
 * @module types/announcement
 */

/** One published announcement, as `GET /api/portal/client/announcements` lists it. */
export interface Announcement {
  /** Announcement id, used as the detail-page route parameter. */
  id: string | number
  /** Headline. */
  title: string
  /** Publication date, already formatted by the backend for display. */
  date: string
  /** Short plain-text lead-in shown in the list. */
  excerpt?: string
  /** Full body as HTML — rendered through `UiRichText`, which rebuilds it from an allow-list of tags. */
  body?: string
}
