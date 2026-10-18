/**
 * Pinia store for the tickets of one client profile.
 *
 * Lists, opens, creates, updates, deletes and answers tickets through `useClientTicketsApi`.
 * A refusal becomes the API sentence in `error` (and the per-field messages in
 * `fieldErrors`); the statistics and department lookups are non-critical and fail silently.
 */

import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useClientTicketsApi } from '@/composables/apis/useClientTicketsApi'
import { runAction } from '@/lib/runAction'
import type { ClientTicketStats } from '@/types/client'
import type { Department } from '@/types/department'
import type { CreateTicketPayload, Ticket, TicketListItem, UpdateTicketPayload } from '@/types/ticket'

/** Loads and changes the support tickets of a client. */
export const useClientTicketsStore = defineStore('clientTickets', () => {
  const api = useClientTicketsApi()

  /** Paginated list of tickets for the current client. */
  const tickets = ref<TicketListItem[]>([])

  /** Total count of tickets matching the current query. */
  const totalCount = ref(0)

  /** Ticket statistics for summary cards. */
  const stats = ref<ClientTicketStats>({ openedThisMonth: 0, openedLastMonth: 0, openedThisYear: 0, openedLastYear: 0 })

  /** Currently loaded ticket detail. */
  const currentTicket = ref<Ticket | null>(null)

  /** Available support departments. */
  const departments = ref<Department[]>([])

  /** True while any API request is in flight. */
  const loading = ref(false)

  /** Sentence for the last failed request, null when none. */
  const error = ref<string | null>(null)

  /** Per-field messages of the last refused request, keyed by camelCase field name. */
  const fieldErrors = ref<Record<string, string>>({})

  /** Throwaway error ref: the stats and department lookups are non-critical, so their failure is silent. */
  const quiet = ref<string | null>(null)

  /**
   * Fetches a paginated list of tickets for a client.
   *
   * @param clientId - The client identifier.
   * @param page - One-based page number.
   * @param pageSize - Number of items per page.
   * @param search - Optional subject search term.
   * @returns Promise that resolves when tickets are loaded.
   */
  const fetchClientTickets = async (
    clientId: number | string,
    page = 1,
    pageSize = 20,
    search?: string,
  ): Promise<void> => {
    loading.value = true
    const result = await runAction(error, () => api.list(clientId, page, pageSize, search))
    loading.value = false
    if (result.ok) {
      tickets.value = result.value.items
      totalCount.value = result.value.totalCount
    }
  }

  /**
   * Fetches ticket statistics for a client summary cards.
   *
   * @param clientId - The client identifier.
   * @returns Promise that resolves when the statistics are loaded.
   */
  const fetchStats = async (clientId: number | string): Promise<void> => {
    const result = await runAction(quiet, () => api.stats(clientId))
    if (result.ok) {
      stats.value = result.value
    }
  }

  /**
   * Fetches a single ticket by id with all its replies.
   *
   * @param id - The ticket identifier.
   * @returns Promise that resolves when the ticket is loaded.
   */
  const fetchById = async (id: number | string): Promise<void> => {
    loading.value = true
    currentTicket.value = null
    const result = await runAction(error, () => api.get(id))
    loading.value = false
    if (result.ok) {
      currentTicket.value = result.value
    }
  }

  /**
   * Fetches all support departments for dropdown options.
   *
   * @returns Promise that resolves when the departments are loaded.
   */
  const fetchDepartments = async (): Promise<void> => {
    const result = await runAction(quiet, () => api.departments())
    if (result.ok) {
      departments.value = result.value
    }
  }

  /**
   * Creates a new ticket on behalf of a client.
   *
   * @param payload - Ticket creation data.
   * @returns The id of the new ticket, or null when refused (see `error`).
   */
  const createTicket = async (payload: CreateTicketPayload): Promise<number | null> => {
    loading.value = true
    const result = await runAction(error, () => api.create(payload), fieldErrors)
    loading.value = false
    return result.ok ? result.value : null
  }

  /**
   * Updates ticket metadata.
   *
   * @param id - The ticket identifier.
   * @param payload - Fields to update.
   * @returns True when saved; false when refused (see `error`).
   */
  const updateTicket = async (id: number | string, payload: UpdateTicketPayload): Promise<boolean> => {
    const result = await runAction(error, () => api.update(id, payload), fieldErrors)
    return result.ok
  }

  /**
   * Permanently deletes a ticket.
   *
   * @param id - The ticket identifier.
   * @returns True when deleted; false when refused (see `error`).
   */
  const deleteTicket = async (id: number | string): Promise<boolean> => {
    const result = await runAction(error, () => api.remove(id))
    return result.ok
  }

  /**
   * Adds a staff reply to a ticket.
   *
   * @param id - The ticket identifier.
   * @param message - The reply message body.
   * @returns True when sent; false when refused (see `error`).
   */
  const replyToTicket = async (id: number | string, message: string): Promise<boolean> => {
    const result = await runAction(error, () => api.reply(id, message), fieldErrors)
    return result.ok
  }

  return {
    tickets,
    totalCount,
    stats,
    currentTicket,
    departments,
    loading,
    error,
    fieldErrors,
    fetchClientTickets,
    fetchStats,
    fetchById,
    fetchDepartments,
    createTicket,
    updateTicket,
    deleteTicket,
    replyToTicket,
  }
})
