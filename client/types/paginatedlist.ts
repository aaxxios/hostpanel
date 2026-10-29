/**
 * The backend's single page shape, the envelope every paged list endpoint answers with.
 *
 * @module types/paginatedlist
 */

/** One page of a backend list. */
export interface PaginatedList<T> {
  /** The items on this page. */
  items: T[]
  /** The 1-based page number this envelope describes. */
  pageNumber: number
  /** How many items a full page holds. */
  pageSize: number
  /** Total items across all pages. */
  totalCount: number
  /** Total number of pages. */
  totalPages: number
}
