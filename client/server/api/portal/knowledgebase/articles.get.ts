/**
 * GET /api/portal/knowledgebase/articles
 *
 * Returns one page of published knowledgebase articles. The query string (`category`, `search`,
 * `page`, `pageSize`) is forwarded unchanged to `GET /api/knowledgebase/articles`, which filters
 * and pages in the backend and answers `KbArticleSummaryDto` rows (no body); the page is passed
 * through as sent.
 *
 * @module server/api/portal/knowledgebase/articles.get
 */

import type { KbArticleSummary } from '~/types/kbarticlesummary'
import type { PaginatedList } from '~/types/paginatedlist'

/** The query parameters the backend listing reads; anything else is not forwarded. */
const FORWARDED_PARAMS = ['category', 'search', 'page', 'pageSize'] as const

export default defineEventHandler(async (event): Promise<PaginatedList<KbArticleSummary>> => {
  const query = getQuery(event)
  const params = new URLSearchParams()
  for (const name of FORWARDED_PARAMS) {
    if (query[name] !== undefined && query[name] !== '') {
      params.set(name, String(query[name]))
    }
  }

  return await internalApiCall<PaginatedList<KbArticleSummary>>(event, `/knowledgebase/articles?${params}`)
})
