/**
 * One function per endpoint on the API payment-gateway log surface (`/api/gateway-logs`). No
 * state lives here, no error handling.
 *
 * The caller is the gateway log store, never a `.vue` file.
 */

import { buildQuery, useApi } from '@/composables/useApi'
import type { PagedResult } from '@/types/pagedresult'
import type { GatewayLogEntry, GatewayLogFilters } from '@/types/gatewaylog'

/**
 * Endpoint functions for the gateway log.
 *
 * @returns One typed function per gateway log endpoint the admin panel uses.
 */
export const useGatewayLogApi = () => {
  const { request } = useApi()

  /**
   * Lists one page of the gateway log, newest first.
   *
   * @param filters - Date range, gateway, debug text and result; empty values are not sent.
   * @param page - Page number (1-based).
   * @param pageSize - Entries per page.
   * @returns One page of log entries.
   */
  const list = (filters: GatewayLogFilters, page: number, pageSize: number): Promise<PagedResult<GatewayLogEntry>> =>
    request<PagedResult<GatewayLogEntry>>(
      `/gateway-logs${buildQuery({
        page,
        pageSize,
        from: filters.from === '' ? '' : `${filters.from}T00:00:00Z`,
        to: filters.to === '' ? '' : `${filters.to}T23:59:59.999Z`,
        gateway: filters.gateway,
        debugData: filters.debugData,
        result: filters.result,
      })}`,
    )

  return { list }
}
