/** One knowledgebase category, as `GET /api/portal/knowledgebase/categories` lists it (the backend's `PublishedKbCategoryDto`). */
export interface KbCategory {
  /** Category name; the published articles carry it, and the category page routes on it. */
  name: string
  /** How many published articles the category holds, shown beside the name. */
  articleCount: number
}
