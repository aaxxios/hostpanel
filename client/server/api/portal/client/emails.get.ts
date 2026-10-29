/**
 * GET /api/portal/client/emails
 * Returns the authenticated client's email history: the backend's page of `MyEmailLogDto` rows,
 * passed through as sent.
 *
 * 100 is the API's per-page ceiling; asking for more silently returns 100, so the number is
 * spelled out here rather than left to look like an unlimited request. The account screen
 * paginates that page client-side.
 *
 * @module server/api/portal/client/emails.get
 */

import type { ClientEmailSummary } from '~/types/clientemailsummary'
import type { PaginatedList } from '~/types/paginatedlist'

export default defineEventHandler(async (event): Promise<PaginatedList<ClientEmailSummary>> =>
  await internalApiCall<PaginatedList<ClientEmailSummary>>(event, '/me/emails?page=1&pageSize=100')
)
