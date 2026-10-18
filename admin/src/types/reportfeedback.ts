/** One feedback comment left on a ticket. */
export interface ReportFeedbackCommentRow {
  /** Ticket id. */
  ticketId: number
  /** Staff member rated. */
  staffName: string
  /** Ticket subject. */
  subject: string
  /** The comment, when one was left. */
  feedbackComment: string | null
  /** Rating, 1 to 10. */
  rating: number
  /** Who left the feedback. */
  feedbackLeftBy: string
  /** When it was left. */
  feedbackAt: string
}

/** The ticket feedback comments report. */
export interface ReportFeedbackComments {
  /** The comments. */
  rows: ReportFeedbackCommentRow[]
}

/** Rating counts of one staff member, one counter per rating from 1 to 10. */
export interface ReportFeedbackScoreRow {
  /** Staff member. */
  staffName: string
  /** Count of rating 1. */
  r1: number
  /** Count of rating 2. */
  r2: number
  /** Count of rating 3. */
  r3: number
  /** Count of rating 4. */
  r4: number
  /** Count of rating 5. */
  r5: number
  /** Count of rating 6. */
  r6: number
  /** Count of rating 7. */
  r7: number
  /** Count of rating 8. */
  r8: number
  /** Count of rating 9. */
  r9: number
  /** Count of rating 10. */
  r10: number
  /** Number of ratings. */
  totalRatings: number
  /** Average rating. */
  averageRating: number
}

/** The ticket feedback scores report. */
export interface ReportFeedbackScores {
  /** One row per staff member. */
  rows: ReportFeedbackScoreRow[]
}
