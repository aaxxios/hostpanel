/** A download category as the API returns it. */
export interface DownloadCategory {
  /** Unique category identifier. */
  id: number
  /** Category display name. */
  name: string
  /** Category description text. */
  description: string
  /** Whether the category is hidden from clients. */
  isHidden: boolean
  /** Parent category ID, null for top-level categories. */
  parentCategoryId: number | null
  /** Number of downloads in this category. */
  downloadCount: number
}

/** Body that creates or replaces a download category. */
export interface DownloadCategoryPayload {
  /** Category display name. */
  name: string
  /** Category description text. */
  description: string
  /** Whether the category is hidden from clients. */
  isHidden: boolean
  /** Parent category ID, null for a top-level category. */
  parentCategoryId: number | null
}
