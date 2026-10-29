/**
 * The fields a client sends to open a support ticket.
 *
 * @module types/newticket
 */

import type { TicketPriority } from '~/types/ticketpriority'

/** The fields `POST /api/portal/client/tickets` needs to open a support ticket (`CreateTicketCommand`). */
export interface NewTicket {
  /** Department the ticket is routed to (the backend's `CreateTicketCommand.DepartmentId`); `null` until one is chosen, so the API can refuse it on this field. */
  departmentId: number | null
  /** Ticket subject line. */
  subject: string
  /** First message on the ticket. */
  message: string
  /** Priority the client chose. */
  priority: TicketPriority
}
