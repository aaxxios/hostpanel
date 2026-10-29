/**
 * One support ticket on the client account, as the ticket list returns it.
 *
 * @module types/clientticket
 */

import type { TicketPriority } from '~/types/ticketpriority'
import type { TicketStatus } from '~/types/ticketstatus'

/**
 * One support ticket on the client's account, as `GET /api/portal/client/tickets` lists it.
 * Mirrors the backend `MyTicketListItemDto` one to one (the BFF forwards it unmapped); the staff
 * flag and the owner fields are not part of it.
 */
export interface ClientTicket {
  /** Ticket primary key. */
  id: number
  /** Ticket subject line. */
  subject: string
  /** Current status, as the backend's `TicketStatus` member name. */
  status: TicketStatus
  /** Priority, as the backend's `TicketPriority` member name. */
  priority: TicketPriority
  /** When the ticket was opened (ISO 8601). */
  createdAt: string
  /** Number of replies on the ticket. */
  replyCount: number
  /** Display name of the department handling the ticket, if any. */
  departmentName: string | null
  /** When the last reply was posted (ISO 8601); null while nobody has replied. */
  lastReplyAt: string | null
}
