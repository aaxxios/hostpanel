/**
 * A support ticket together with its whole conversation.
 *
 * @module types/ticketthread
 */

import type { TicketPriority } from '~/types/ticketpriority'
import type { TicketReply } from '~/types/ticketreply'
import type { TicketStatus } from '~/types/ticketstatus'

/**
 * One support ticket with its whole conversation, as `GET /api/portal/client/tickets/:id`
 * returns it. Mirrors the backend `MyTicketDto` one to one (the BFF forwards it unmapped); the
 * staff assignment, flag and tags are not part of it.
 */
export interface TicketThread {
  /** Ticket primary key. */
  id: number
  /** Ticket subject line. */
  subject: string
  /** The opening message. */
  message: string
  /** Current status, as the backend's `TicketStatus` member name. */
  status: TicketStatus
  /** Priority, as the backend's `TicketPriority` member name. */
  priority: TicketPriority
  /** FK to the department handling the ticket, if any. */
  departmentId: number | null
  /** Display name of that department, if any. */
  departmentName: string | null
  /** When the ticket was opened (ISO 8601). */
  createdAt: string
  /** The replies, oldest first; empty on a ticket nobody has answered yet. */
  replies: TicketReply[]
  /** Satisfaction rating left on the ticket, if any. */
  rating: number | null
  /** Comment that came with the rating, if any. */
  feedbackComment: string | null
  /** When the feedback was left (ISO 8601), if it was. */
  feedbackAt: string | null
}
