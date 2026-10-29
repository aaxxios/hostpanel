/**
 * Pinia store for the help surface: opening a ticket, replying to one, and the public contact
 * form.
 *
 * The store owns the loading flag and the refusal sentence of each submission, so a page only
 * renders them and branches on the returned {@link SubmitOutcome}: it never wraps an API call
 * in `try/catch`. Same pattern as `signIn` in `stores/auth.ts`.
 *
 * URLs and transport belong to {@link useSupportApi}.
 *
 * @module stores/support
 */

import { defineStore } from 'pinia'
import { useSupportApi } from '~/composables/apis/useSupportApi'
import type { ContactMessage } from '~/types/contactmessage'
import type { NewTicket } from '~/types/newticket'
import type { SupportDepartment } from '~/types/supportdepartment'
import type { TicketThread } from '~/types/ticketthread'
import { SubmitOutcome } from '~/types/submitoutcome'
import { apiErrorMessage, apiFieldErrors } from '~/lib/errorHandler'

/**
 * Central store for support submissions.
 *
 * @returns The submission state and the actions that change it.
 */
export const useSupportStore = defineStore('support', () => {
  const api = useSupportApi()

  /**
   * Field-level messages of the last refused new-ticket or contact-form post, keyed by camelCase field name (`UiField` renders
   * `fieldErrors[name]`); empty when the refusal was not about fields or nothing was refused.
   */
  const fieldErrors = ref<Record<string, string>>({})

  /** The ticket on screen with its thread; null until {@link loadTicket} or after a failed read. */
  const ticket = ref<TicketThread | null>(null)

  /** The id of the ticket held in {@link ticket}, so a different one is not shown while it loads. */
  const ticketId = ref('')

  /** True while a ticket is being read. */
  const ticketReading = ref(false)

  /** The sentence for the last failed ticket read; empty when there is none to show. */
  const ticketReadError = ref('')

  /** The departments a ticket can be routed to; empty until {@link loadDepartments}. */
  const departments = ref<SupportDepartment[]>([])

  /** True while the departments are being read. */
  const departmentsLoading = ref(false)

  /**
   * Reads one ticket with its thread.
   *
   * @param id - Ticket id.
   * @returns The ticket; null when it could not be read ({@link ticketReadError} holds the
   * sentence). Re-reading the ticket already on screen keeps it visible meanwhile, so a new
   * reply appears without the page blinking.
   */
  const loadTicket = async (id: string): Promise<TicketThread | null> => {
    ticketReading.value = true
    ticketReadError.value = ''
    if (ticketId.value !== id) {
      ticket.value = null
    }
    try {
      ticket.value = await api.fetchTicket(id)
      ticketId.value = id
    } catch (e: unknown) {
      ticket.value = null
      ticketReadError.value = apiErrorMessage(e)
    } finally {
      ticketReading.value = false
    }
    return ticket.value
  }

  /**
   * Reads the departments the new-ticket form offers. A failure leaves the list empty, which
   * the picker renders as having nothing to choose.
   *
   * @returns The departments.
   */
  const loadDepartments = async (): Promise<SupportDepartment[]> => {
    departmentsLoading.value = true
    try {
      departments.value = await api.fetchDepartments()
    } catch {
      departments.value = []
    } finally {
      departmentsLoading.value = false
    }
    return departments.value
  }

  /** True while a new ticket is being sent. */
  const ticketLoading = ref(false)

  /** The sentence explaining why the last new-ticket attempt failed, or `''`. */
  const ticketError = ref('')

  /** True while a ticket reply is being sent. */
  const replyLoading = ref(false)

  /** The sentence explaining why the last reply attempt failed, or `''`. */
  const replyError = ref('')

  /** True while the contact form is being sent. */
  const contactLoading = ref(false)

  /** The sentence explaining why the last contact attempt failed, or `''`. */
  const contactError = ref('')

  /**
   * Opens a support ticket, keeping the failure as state instead of throwing.
   *
   * @param ticket - Department, subject, first message and priority.
   * @returns How the attempt ended; on {@link SubmitOutcome.Failed} {@link ticketError} holds
   * the sentence to show.
   */
  const createTicket = async (ticket: NewTicket): Promise<SubmitOutcome> => {
    ticketError.value = ''
    fieldErrors.value = {}
    ticketLoading.value = true
    try {
      await api.createTicket(ticket)
      return SubmitOutcome.Sent
    } catch (e: unknown) {
      ticketError.value = apiErrorMessage(e)
      fieldErrors.value = apiFieldErrors(e)
      return SubmitOutcome.Failed
    } finally {
      ticketLoading.value = false
    }
  }

  /**
   * Posts a reply onto a ticket, keeping the failure as state instead of throwing.
   *
   * @param id - Ticket id.
   * @param message - The reply body.
   * @returns How the attempt ended; on {@link SubmitOutcome.Failed} {@link replyError} holds
   * the sentence to show.
   */
  const replyToTicket = async (id: string, message: string): Promise<SubmitOutcome> => {
    replyError.value = ''
    replyLoading.value = true
    try {
      await api.replyToTicket(id, message)
      return SubmitOutcome.Sent
    } catch (e: unknown) {
      replyError.value = apiErrorMessage(e)
      return SubmitOutcome.Failed
    } finally {
      replyLoading.value = false
    }
  }

  /**
   * Sends the public contact form, keeping the failure as state instead of throwing.
   *
   * @param message - The form's fields.
   * @returns How the attempt ended; on {@link SubmitOutcome.Failed} {@link contactError} holds
   * the sentence to show.
   */
  const sendContactMessage = async (message: ContactMessage): Promise<SubmitOutcome> => {
    contactError.value = ''
    fieldErrors.value = {}
    contactLoading.value = true
    try {
      await api.sendContactMessage(message)
      return SubmitOutcome.Sent
    } catch (e: unknown) {
      contactError.value = apiErrorMessage(e)
      fieldErrors.value = apiFieldErrors(e)
      return SubmitOutcome.Failed
    } finally {
      contactLoading.value = false
    }
  }

  return {
    ticket,
    ticketReading,
    ticketReadError,
    loadTicket,
    departments,
    departmentsLoading,
    loadDepartments,
    fieldErrors,
    ticketLoading,
    ticketError,
    replyLoading,
    replyError,
    contactLoading,
    contactError,
    createTicket,
    replyToTicket,
    sendContactMessage
  }
})
