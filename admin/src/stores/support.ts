/**
 * Pinia store for the global Support module: tickets, replies, departments and the overview.
 *
 * Owns every request the support pages make and the sentence for a refusal (the API's own,
 * see `lib/errorHandler.ts`), so no support page holds a `catch`.
 */
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useSupportApi } from '@/composables/apis/useSupportApi'
import { runAction } from '@/lib/runAction'
import type { Department } from '@/types/department'
import type { SupportOverviewPeriod, SupportOverviewStats } from '@/types/supportoverviewstats'
import type {
  CreateTicketPayload,
  Ticket,
  TicketBulkAction,
  TicketListFilter,
  TicketListItem,
  UpdateTicketPayload,
} from '@/types/ticket'
import { DEFAULT_PAGE_SIZE } from '@/lib/limits'

/**
 * Paginated ticket listing with status/search filters, overview stats, create/update/delete,
 * replies, flagging and bulk actions.
 */
export const useSupportStore = defineStore('support', () => {
  const api = useSupportApi()

  /** Paginated list of tickets. */
  const tickets = ref<TicketListItem[]>([])

  /** Total ticket count matching the current filter. */
  const totalCount = ref(0)

  /** Currently loaded ticket detail. */
  const currentTicket = ref<Ticket | null>(null)

  /** Available support departments. */
  const departments = ref<Department[]>([])

  /** Support overview statistics. */
  const overviewStats = ref<SupportOverviewStats | null>(null)

  /** True while a load or create request is in flight. */
  const loading = ref(false)

  /** Sentence for the last failed call, null when none. */
  const error = ref<string | null>(null)

  /** Per-field messages of the last refused create, keyed by camelCase field name. */
  const fieldErrors = ref<Record<string, string>>({})

  /** Current page (1-based). */
  const page = ref(1)

  /** Items per page. */
  const pageSize = ref(DEFAULT_PAGE_SIZE)

  /** Active status filter (null = all). */
  const statusFilter = ref<TicketListFilter | null>(null)

  /** Subject search term. */
  const search = ref('')

  /**
   * Loads the filtered, paged ticket list.
   *
   * @returns True when loaded; false when refused (see `error`).
   */
  const fetchAll = async (): Promise<boolean> => {
    loading.value = true
    const result = await runAction(error, () =>
      api.listTickets({
        page: page.value,
        pageSize: pageSize.value,
        status: statusFilter.value,
        search: search.value,
      }),
    )
    loading.value = false
    if (result.ok) {
      tickets.value = result.value.items
      totalCount.value = result.value.totalCount
    }
    return result.ok
  }

  /**
   * Loads one ticket with its replies into `currentTicket`.
   *
   * @param id - The ticket identifier.
   * @returns True when loaded; false when refused (see `error`).
   */
  const fetchById = async (id: number | string): Promise<boolean> => {
    loading.value = true
    currentTicket.value = null
    const result = await runAction(error, () => api.getTicket(id))
    loading.value = false
    if (result.ok) {
      currentTicket.value = result.value
    }
    return result.ok
  }

  /**
   * Loads the overview figures for a period.
   *
   * @param period - The reporting period.
   * @returns True when loaded; false when refused (see `error`).
   */
  const fetchOverview = async (period: SupportOverviewPeriod): Promise<boolean> => {
    loading.value = true
    const result = await runAction(error, () => api.getOverview(period))
    loading.value = false
    if (result.ok) {
      overviewStats.value = result.value
    }
    return result.ok
  }

  /**
   * Loads the departments for dropdown options. A failure leaves the list as it was and is not
   * shown: the pages work without the dropdown being filled.
   *
   * @returns True when loaded.
   */
  const fetchDepartments = async (): Promise<boolean> => {
    /** Throwaway error ref: the departments are optional, so their failure is deliberately silent. */
    const quiet = ref<string | null>(null)
    const result = await runAction(quiet, () => api.listDepartments())
    if (result.ok) {
      departments.value = result.value
    }
    return result.ok
  }

  /**
   * Opens a ticket on behalf of a client.
   *
   * @param payload - Ticket creation data.
   * @returns The new ticket's id, or null when refused (see `error` and `fieldErrors`).
   */
  const createTicket = async (payload: CreateTicketPayload): Promise<number | null> => {
    loading.value = true
    const result = await runAction(error, () => api.createTicket(payload), fieldErrors)
    loading.value = false
    return result.ok ? result.value : null
  }

  /**
   * Changes a ticket's metadata.
   *
   * @param id - The ticket identifier.
   * @param payload - Fields to update.
   * @returns True when accepted; false when refused (see `error`).
   */
  const updateTicket = async (id: number | string, payload: UpdateTicketPayload): Promise<boolean> =>
    (await runAction(error, () => api.updateTicket(id, payload))).ok

  /**
   * Permanently deletes a ticket.
   *
   * @param id - The ticket identifier.
   * @returns True when deleted; false when refused (see `error`).
   */
  const deleteTicket = async (id: number | string): Promise<boolean> =>
    (await runAction(error, () => api.deleteTicket(id))).ok

  /**
   * Adds a staff reply to a ticket.
   *
   * @param id - The ticket identifier.
   * @param message - The reply message body.
   * @returns True when sent; false when refused (see `error`).
   */
  const replyToTicket = async (id: number | string, message: string): Promise<boolean> =>
    (await runAction(error, () => api.replyToTicket(id, { message }))).ok

  /**
   * Flips the flagged state of a ticket.
   *
   * @param id - The ticket identifier.
   * @returns True when toggled; false when refused (see `error`).
   */
  const toggleFlag = async (id: number | string): Promise<boolean> =>
    (await runAction(error, () => api.toggleFlag(id))).ok

  /**
   * Runs one action on several tickets.
   *
   * @param ticketIds - The tickets the action applies to.
   * @param action - The bulk action to run.
   * @returns True when accepted; false when refused (see `error`).
   */
  const bulkAction = async (ticketIds: number[], action: TicketBulkAction): Promise<boolean> =>
    (await runAction(error, () => api.bulkAction({ ticketIds, action }))).ok

  return {
    tickets,
    totalCount,
    currentTicket,
    departments,
    overviewStats,
    loading,
    error,
    fieldErrors,
    page,
    pageSize,
    statusFilter,
    search,
    fetchAll,
    fetchById,
    fetchOverview,
    fetchDepartments,
    createTicket,
    updateTicket,
    deleteTicket,
    replyToTicket,
    toggleFlag,
    bulkAction,
  }
})
