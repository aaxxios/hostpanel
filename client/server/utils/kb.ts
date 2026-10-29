/**
 * Mapping for the knowledgebase article BFF route under `server/api/portal/knowledgebase/`.
 *
 * The C# backend answers `PublishedKbArticleDto` (`{ id, title, content, category }`)
 * while the public pages read the legacy-shaped `KbArticle` (`body`, `excerpt`). One mapping
 * lives here, so a field added on either side is changed once.
 *
 * @module server/utils/kb
 */

import type { BackendKbArticle } from '~/types/backendkbarticle'
import type { KbArticle } from '~/types/kbarticle'

/** How many characters of plain text a list row's excerpt carries. */
const EXCERPT_LENGTH = 140

/**
 * Reduces an article body to the plain-text lead-in a list row shows.
 *
 * The body is HTML, and a row renders its excerpt as text, so tags are stripped rather than
 * escaped: an excerpt that kept its markup would print the tags at the reader.
 *
 * @param html - The article body.
 * @returns Up to {@link EXCERPT_LENGTH} characters of collapsed plain text.
 */
const toExcerpt = (html: string): string =>
  (html ?? '')
    .replace(/<[^>]*>/g, ' ')
    .replace(/\s+/g, ' ')
    .trim()
    .slice(0, EXCERPT_LENGTH)

/**
 * Maps one backend article onto the shape the knowledgebase pages render.
 *
 * `views` and `dateupdated` are deliberately absent: this backend records neither, and the
 * pages hide both when they are missing. Filling them with zeroes or today's date would put
 * a number on screen that means nothing.
 *
 * @param article - The article as the backend returned it.
 * @returns The article in the page's shape, body included.
 */
export const toKbArticle = (article: BackendKbArticle): KbArticle => ({
  id: article.id,
  title: article.title,
  excerpt: toExcerpt(article.content),
  body: article.content
})
