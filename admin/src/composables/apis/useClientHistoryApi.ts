/**
 * One function per history endpoint of a client profile: the activity log
 * (`/api/clients/{id}/activity-logs`) and the e-mail log (`/api/admin/email-logs/client/{id}`).
 * No state lives here, no error handling.
 *
 * Callers are the client log and client email stores, never a `.vue` file.
 */

import { buildQuery, useApi } from '@/composables/useApi'
import type { ActivityLog, ActivityLogFilters } from '@/types/activitylog'
import type { EmailLog } from '@/types/emaillog'
import { ApiPath } from '@/lib/apiPaths'

/** The paged envelope both history endpoints answer with. */
interface HistoryPage<T> {
  /** Entries on the requested page. */
  items: T[]
  /** Total number of matching entries. */
  totalCount: number
}

/**
 * Endpoint functions for the client history views.
 *
 * @returns One typed function per history endpoint.
 */
export const useClientHistoryApi = () => {
  const { request } = useApi()

  /**
   * Lists the activity log of a client.
   *
   * @param clientId - The client id.
   * @param page - Page number (1-based).
   * @param pageSize - Entries per page.
   * @param filters - Optional date, admin, description and IP filters.
   * @returns One page of entries.
   */
  const activityLogs = (
    clientId: number,
    page: number,
    pageSize: number,
    filters: ActivityLogFilters = {},
  ): Promise<HistoryPage<ActivityLog>> =>
    request<HistoryPage<ActivityLog>>(`${ApiPath.Clients}/${clientId}/activity-logs${buildQuery({ page, pageSize, ...filters })}`)

  /**
   * Lists the e-mails sent to a client.
   *
   * @param clientId - The client id.
   * @param page - Page number (1-based).
   * @param pageSize - Entries per page.
   * @returns One page of entries.
   */
  const emailLogs = (clientId: number, page: number, pageSize: number): Promise<HistoryPage<EmailLog>> =>
    request<HistoryPage<EmailLog>>(`${ApiPath.AdminEmailLogs}/client/${clientId}${buildQuery({ page, pageSize })}`)

  return { activityLogs, emailLogs }
}
