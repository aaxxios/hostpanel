/**
 * The filters and paging `GET /api/portal/knowledgebase/articles` accepts.
 *
 * @module types/kbarticlequery
 */

/** The filters and paging `GET /api/portal/knowledgebase/articles` accepts. */
export interface KbArticleQuery {
  /** Restrict to one category, by name. Omit for every category. */
  category?: string
  /** Free-text search term. Omit to list rather than search. */
  search?: string
  /** 1-based page number. Omit for the first page. */
  page?: number
  /** How many articles a page holds, 1 to 100. */
  pageSize?: number
}
