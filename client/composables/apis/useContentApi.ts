/**
 * Endpoints for published editorial content: announcements and the knowledgebase.
 *
 * Every function returns a plain `Promise` through `apiFetch`, so `stores/content.ts` can call
 * them from a store action, including the action a server-rendered page wraps in
 * `useAsyncData`. Nothing here is cached, computed or re-read: the store keeps the state.
 *
 * @module composables/apis/useContentApi
 */

import { apiFetch } from '~/composables/useApi'
import type { KbArticle } from '~/types/kbarticle'
import type { KbArticleSummary } from '~/types/kbarticlesummary'
import type { KbArticleQuery } from '~/types/kbarticlequery'
import type { KbCategory } from '~/types/kbcategory'
import type { PaginatedList } from '~/types/paginatedlist'
import type { PublishedAnnouncement } from '~/types/publishedannouncement'

/**
 * The announcements and knowledgebase surface, one function per endpoint.
 *
 * @returns The content endpoint functions.
 */
export const useContentApi = () => {
  /**
   * Reads the announcement list.
   *
   * @returns The first page of published announcements.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchAnnouncements = (): Promise<PaginatedList<PublishedAnnouncement>> =>
    apiFetch<PaginatedList<PublishedAnnouncement>>('/api/portal/client/announcements')

  /**
   * Reads the ten most recent published announcements for the dashboard card.
   *
   * @returns The first page of ten published announcements, newest first.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchRecentAnnouncements = (): Promise<PaginatedList<PublishedAnnouncement>> =>
    apiFetch<PaginatedList<PublishedAnnouncement>>('/api/portal/public/announcements')

  /**
   * Reads one published announcement, body included, from the public detail route.
   *
   * @param id - Announcement id.
   * @returns The announcement.
   * @throws Whatever `apiFetch` throws; an unknown or unpublished id is a refusal.
   */
  const fetchAnnouncement = (id: string): Promise<PublishedAnnouncement> =>
    apiFetch<PublishedAnnouncement>(`/api/portal/public/announcements/${id}`)

  /**
   * Reads the knowledgebase categories.
   *
   * @returns The categories.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchKbCategories = (): Promise<KbCategory[]> =>
    apiFetch<KbCategory[]>('/api/portal/knowledgebase/categories')

  /**
   * Reads one page of knowledgebase articles.
   *
   * @param query - Filters and paging.
   * @returns The page of articles.
   * @throws Whatever `apiFetch` throws.
   */
  const fetchKbArticles = (query: KbArticleQuery): Promise<PaginatedList<KbArticleSummary>> =>
    apiFetch<PaginatedList<KbArticleSummary>>('/api/portal/knowledgebase/articles', { query })

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
    fetchAnnouncement,
    fetchKbCategories,
    fetchKbArticles,
    fetchKbArticle
  }
}
