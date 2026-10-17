/** One published article exactly as `GET /api/knowledgebase` returns it. */
export interface BackendKbArticle {
  /** Article primary key. */
  id: number
  /** Article title. */
  title: string
  /** Full article body as HTML. */
  content: string
  /** Category name -- categories have no id of their own in this backend. */
  category: string
  /** Whether the article is visible to clients. The public list returns only `true`. */
  isPublished: boolean
}
