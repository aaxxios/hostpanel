/**
 * A support ticket together with its whole conversation.
 *
 * @module types/ticketthread
 */

import type { TicketReply } from '~/types/ticketreply'

/**
 * One support ticket with its whole conversation, as `GET /api/portal/client/tickets/:id`
 * returns it. Mirrors the backend `TicketDto` one to one (the BFF forwards it unmapped).
 */
export interface TicketThread {
  /** Ticket primary key. */
  id: number
  /** FK to the client who opened the ticket. */
  clientId: number
  /** Ticket subject line. */
  subject: string
  /** The opening message. */
  message: string
  /** Current status, as the backend's `TicketStatus` member name, e.g. "Open", "Answered", "Closed". */
  status: string
  /** Priority, as the backend's `TicketPriority` member name, e.g. "Medium". */
  priority: string
  /** FK to the department handling the ticket, if any. */
  departmentId: number | null
  /** Display name of that department, if any. */
  departmentName: string | null
  /** FK to the staff member the ticket is assigned to, if any. */
  assignedToStaffId: number | null
  /** Display name of that staff member, if any. */
  assignedToStaffName: string | null
  /** When the ticket was opened (ISO 8601). */
  createdAt: string
  /** Whether staff flagged the ticket. */
  isFlagged: boolean
  /** The replies, oldest first; empty on a ticket nobody has answered yet. */
  replies: TicketReply[]
  /** Tags staff put on the ticket. */
  tags: string[]
  /** Satisfaction rating left on the ticket, if any. */
  rating: number | null
  /** Comment that came with the rating, if any. */
  feedbackComment: string | null
  /** Who left the feedback, if anyone. */
  feedbackLeftBy: string | null
  /** When the feedback was left (ISO 8601), if it was. */
  feedbackAt: string | null
}
