/** One page of a list endpoint, the single page shape every migrated endpoint answers with. */
export interface PaginatedList<T> {
  /** Data items for the current page. */
  items: T[]
  /** Current page number (1-based). */
  pageNumber: number
  /** Number of items per page. */
  pageSize: number
  /** Total number of items across all pages. */
  totalCount: number
  /** Total number of pages. */
  totalPages: number
}
