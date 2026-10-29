/** One published article exactly as `GET /api/knowledgebase/{id}` returns it. */
export interface BackendKbArticle {
  /** Article primary key. */
  id: number
  /** Article title. */
  title: string
  /** Full article body as HTML. */
  content: string
  /** Category name: categories have no id of their own in this backend. */
  category: string
}
