/**
 * One row of the published knowledgebase listing.
 *
 * @module types/kbarticlesummary
 */

/** One article as the backend `KbArticleSummaryDto` lists it: no body, a plain-text excerpt instead. */
export interface KbArticleSummary {
  /** Article primary key; the row links to `/knowledgebase/<id>`. */
  id: number
  /** Headline. */
  title: string
  /** The opening of the body as plain text, at most 140 characters; empty for an empty body. */
  excerpt: string
  /** Category the article belongs to. */
  category: string
}
