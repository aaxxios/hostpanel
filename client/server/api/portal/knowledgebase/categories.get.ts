/**
 * GET /api/portal/knowledgebase/categories
 *
 * Returns the categories of the published knowledgebase articles with their article counts.
 * Calls `GET /api/knowledgebase/categories`, which groups and counts in the backend, and passes
 * the list through as sent.
 *
 * @module server/api/portal/knowledgebase/categories.get
 */

import type { KbCategory } from '~/types/kbcategory'

export default defineEventHandler(async (event): Promise<KbCategory[]> =>
  await internalApiCall<KbCategory[]>(event, '/knowledgebase/categories'))
