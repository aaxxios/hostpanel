/** One message on a support ticket thread, as the backend's `TicketReplyDto` sends it. */
export interface TicketReply {
  /** Reply primary key, used as the list key. */
  id: number
  /** The reply body. */
  message: string
  /** Display name of who wrote it; may be an empty string (render with `nameOrUnknown`). */
  authorName: string
  /** Whether a member of staff wrote it. */
  isStaffReply: boolean
  /** When it was posted (ISO 8601). */
  createdAt: string
}
