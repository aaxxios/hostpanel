/** DTO representing an announcement. */
export interface Announcement {
  /** Announcement primary key. */
  id: number
  /** Announcement title. */
  title: string
  /** HTML content body. */
  content: string
  /** Whether the announcement is published. */
  isPublished: boolean
  /** ISO 8601 creation timestamp. */
  createdAt: string
}

/** Body that creates or replaces an announcement. */
export interface AnnouncementPayload {
  /** Announcement title. */
  title: string
  /** HTML content body. */
  content: string
  /** Whether the announcement is published. */
  isPublished: boolean
}
