/** One rated ticket reply. */
export interface ReportRatingRow {
  /** Ticket id. */
  ticketId: number
  /** Reply date. */
  date: string
  /** The reply text. */
  message: string
  /** Staff member who wrote it. */
  adminName: string
  /** Rating, 1 to 10. */
  rating: number
}

/** The ticket ratings reviewer report. */
export interface ReportRatings {
  /** The rated replies. */
  rows: ReportRatingRow[]
}
