/**
 * Fixtures for the gateway log screen.
 */
import type { GatewayLogEntry } from '../../src/types/gatewaylog'
import { GatewayLogResult } from '../../src/types/gatewaylog'
import type { PaginatedList } from '../../src/types/paginatedlist'

/** `GET /api/gateway-logs`: one open session and one settled payment. */
export const gatewayLogPage: PaginatedList<GatewayLogEntry> = {
  items: [
    {
      id: 'session-9',
      dateTime: '2026-10-14T10:00:00Z',
      gateway: 'bank-gateway',
      debugData: 'Invoice #9 order ord-9',
      result: GatewayLogResult.Pending,
      invoiceId: 9,
    },
    {
      id: 'tx-4',
      dateTime: '2026-10-13T09:00:00Z',
      gateway: 'bank-gateway',
      debugData: 'Invoice #4 ref ref-4',
      result: GatewayLogResult.Payment,
      invoiceId: 4,
    },
  ],
  pageNumber: 1,
  pageSize: 20,
  totalCount: 2,
  totalPages: 1,
}
