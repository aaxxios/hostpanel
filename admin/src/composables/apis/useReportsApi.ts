/**
 * Endpoint functions for the report screens.
 *
 * No state lives here: the reports store owns loading and error. Each report screen
 * supplies its own endpoint path (with query string), so these functions take the path
 * and add only the verb and the response type. Every function rejects with an `ApiError`.
 */

import { API_BASE } from '@/lib/apiPaths'
import { useApi } from '@/composables/useApi'
import type { ReportClientOption } from '@/types/reportclientoption'
import { HttpMethod } from '@/types/http'

/**
 * Endpoint functions for `/api/reports/*`.
 *
 * @returns One typed function per kind of report call.
 */
export const useReportsApi = () => {
  const { request } = useApi()

  /**
   * Reads one report.
   *
   * @param path - Report endpoint path, including its query string.
   * @returns The report data.
   */
  const fetchReport = <T>(path: string): Promise<T> => request<T>(path)

  /**
   * Asks the API to recompute a report and return the fresh data.
   *
   * @param path - Report endpoint path, including its query string.
   * @returns The refreshed report data.
   */
  const refreshReport = <T>(path: string): Promise<T> => request<T>(path, { method: HttpMethod.Post })

  /**
   * Reads the clients the report pickers offer.
   *
   * @returns Id and display name per client.
   */
  const fetchClientPicker = (): Promise<ReportClientOption[]> =>
    request<ReportClientOption[]>('/reports/client-picker')

  /**
   * Builds the address of a report's CSV export.
   *
   * A download is a browser navigation, not a request `useApi` can make, so the page needs the
   * address itself; it is the same `/api` prefix the transport uses.
   *
   * @param path - Export endpoint path, including its query string.
   * @returns The address to open.
   */
  const exportUrl = (path: string): string => `${API_BASE}${path}`

  return { fetchReport, refreshReport, fetchClientPicker, exportUrl }
}
