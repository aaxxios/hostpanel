/**
 * Endpoints for getting help: support tickets under `/api/portal/client/tickets`, and the
 * public contact form at `/api/contact`.
 *
 * Every function returns a plain `Promise`. Nothing here is cached or held.
 *
 * @module composables/apis/useSupportApi
 */

import { apiFetch } from '~/composables/useApi'
import type { ContactMessage } from '~/types/contactmessage'
import type { NewTicket } from '~/types/newticket'
import type { SupportDepartment } from '~/types/supportdepartment'
import type { TicketThread } from '~/types/ticketthread'
import { HttpMethod } from '~/types/httpmethod'

/**
 * The support surface, one function per endpoint.
 *
 * @returns The support endpoint functions.
 */
export const useSupportApi = () => {
  /**
   * Reads one ticket with its whole reply thread.
   *
   * @param id - Ticket id.
   * @returns The ticket.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchTicket = (id: string): Promise<TicketThread> =>
    apiFetch<TicketThread>(`/api/portal/client/tickets/${id}`)

  /**
   * Reads the departments a ticket can be routed to.
   *
   * Under `/api/portal/public`, but it belongs to this file rather than to `useCatalogApi`:
   * the list only exists to fill the ticket form's department picker.
   *
   * @returns The departments.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchDepartments = (): Promise<SupportDepartment[]> =>
    apiFetch<SupportDepartment[]>('/api/portal/public/departments')

  /**
   * Opens a support ticket.
   *
   * @param ticket - Department, subject, first message and priority.
   * @returns Whatever the endpoint answers with.
   * @throws Whatever `apiFetch` throws.
   */
  const createTicket = (ticket: NewTicket): Promise<unknown> =>
    apiFetch('/api/portal/client/tickets', { method: HttpMethod.Post, body: ticket })

  /**
   * Posts a reply onto an existing ticket.
   *
   * @param id - Ticket id.
   * @param message - The reply body.
   * @returns Whatever the endpoint answers with.
   * @throws Whatever `apiFetch` throws.
   */
  const replyToTicket = (id: string, message: string): Promise<unknown> =>
    apiFetch(`/api/portal/client/tickets/${id}/reply`, { method: HttpMethod.Post, body: { message } })

  /**
   * Sends a message from the public contact form.
   *
   * Not under `/api/portal`, and the path is kept as it is: it is the public site's own route
   * and moving it would be a change to the site's URLs for no gain. The Nuxt route behind it
   * relays to the C# backend, which sends the mail, so the Nuxt server holds no mail credentials.
   *
   * Resolving means the backend accepted the message. Every failure rejects, carrying the
   * backend's status and `code`: read them with `apiErrorMessage` / `apiErrorCode`.
   *
   * @param message - The form's fields.
   * @returns Nothing; the endpoint answers 204 on success.
   * @throws Whatever `apiFetch` throws.
   */
  const sendContactMessage = (message: ContactMessage): Promise<unknown> =>
    apiFetch('/api/contact', { method: HttpMethod.Post, body: message })

  return { fetchTicket, fetchDepartments, createTicket, replyToTicket, sendContactMessage }
}
