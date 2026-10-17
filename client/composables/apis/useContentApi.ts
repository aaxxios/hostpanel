/**
 * Endpoints for published editorial content — announcements and the knowledgebase.
 *
 * Every function returns a plain `Promise` through `apiFetch`, so `stores/content.ts` can call
 * them from a store action, including the action a server-rendered page wraps in
 * `useAsyncData`. Nothing here is cached, computed or re-read: the store keeps the state.
 *
 * @module composables/apis/useContentApi
 */

import { apiFetch } from '~/composables/useApi'
import type { AnnouncementList } from '~/types/announcementlist'
import type { AnnouncementSummary } from '~/types/announcementsummary'
import type { KbArticle } from '~/types/kbarticle'
import type { KbArticleList } from '~/types/kbarticlelist'
import type { KbArticleQuery } from '~/types/kbarticlequery'
import type { KbCategory } from '~/types/kbcategory'

/**
 * The announcements and knowledgebase surface, one function per endpoint.
 *
 * @returns The content endpoint functions.
 */
export const useContentApi = () => {
  /**
   * Reads the announcement list.
   *
   * @returns The announcement list.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchAnnouncements = (): Promise<AnnouncementList> =>
    apiFetch<AnnouncementList>('/api/portal/client/announcements')

  /**
   * Reads the ten most recent published announcements, flattened for the dashboard card.
   *
   * @returns The announcement summaries, newest first.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchRecentAnnouncements = (): Promise<AnnouncementSummary[]> =>
    apiFetch<AnnouncementSummary[]>('/api/portal/public/announcements')

  /**
   * Reads the knowledgebase categories.
   *
   * @returns The categories.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchKbCategories = (): Promise<KbCategory[]> =>
    apiFetch<KbCategory[]>('/api/portal/knowledgebase/categories')

  /**
   * Reads knowledgebase articles.
   *
   * @param query - Which articles to read.
   * @returns The article list.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchKbArticles = (query: KbArticleQuery): Promise<KbArticleList> =>
    apiFetch<KbArticleList>('/api/portal/knowledgebase/articles', { query })

  /**
   * Reads one knowledgebase article, body included.
   *
   * @param id - Article id.
   * @returns The article.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchKbArticle = (id: string): Promise<KbArticle> =>
    apiFetch<KbArticle>(`/api/portal/knowledgebase/${id}`)

  return {
    fetchAnnouncements,
    fetchRecentAnnouncements,
    fetchKbCategories,
    fetchKbArticles,
    fetchKbArticle
  }
}
