/**
 * One support ticket on the client's account, as `GET /api/portal/client/tickets` lists it.
 * Mirrors the backend `TicketListItemDto` one to one (the BFF forwards it unmapped).
 */
export interface ClientTicket {
  /** Ticket primary key. */
  id: number
  /** FK to the client who opened the ticket. */
  clientId: number
  /** Ticket subject line. */
  subject: string
  /** Current status, as the backend's `TicketStatus` member name, e.g. "Open", "Answered". */
  status: string
  /** Priority, as the backend's `TicketPriority` member name, e.g. "Medium". */
  priority: string
  /** When the ticket was opened (ISO 8601). */
  createdAt: string
  /** Number of replies on the ticket. */
  replyCount: number
  /** Display name of the department handling the ticket, if any. */
  departmentName: string | null
  /** When the last reply was posted (ISO 8601); null while nobody has replied. */
  lastReplyAt: string | null
  /** Whether staff flagged the ticket. */
  isFlagged: boolean
}
