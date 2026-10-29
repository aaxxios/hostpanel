/**
 * The shape of a detail page's title and description for search results and share cards.
 *
 * One place for the separator, so every detail page (knowledge-base article, announcement) writes
 * "Item | Section" the same way, with a plain separator and no dashes.
 *
 * @module utils/seoTitle
 */

import { parseRichText } from '~/utils/ui/richText'
import type { RichNode } from '~/utils/ui/richText'
import { UiRichNodeType } from '~/utils/ui/enums'

/** Separator between the item title and the section name in a page title. */
export const TITLE_SEPARATOR = ' | '

/** Longest description a search result shows before it cuts the text. */
export const DESCRIPTION_MAX_LENGTH = 160

/**
 * Joins an item title and its section name into a page title.
 *
 * @param title The item's own title.
 * @param section The section it belongs to, e.g. the knowledge base.
 * @returns For example `Point a domain | Knowledge base`.
 */
export const detailTitle = (title: string, section: string): string => `${title}${TITLE_SEPARATOR}${section}`

/**
 * Collects the text of a parsed document, one space between blocks.
 *
 * @param nodes Parsed nodes.
 * @returns The text runs joined by single spaces.
 */
const textOf = (nodes: RichNode[]): string[] =>
  nodes.flatMap((node) => (node.type === UiRichNodeType.Text ? [node.text] : textOf(node.children)))

/**
 * Reduces a body of operator HTML to a plain-text description.
 *
 * The body goes through the same allow-list reader the page renders with, so no markup survives
 * into a meta tag.
 *
 * @param html The body from the API.
 * @param maxLength Where to cut the text; an ellipsis character marks a cut.
 * @returns The text with whitespace collapsed, cut at a word boundary when too long; `''` for an empty body.
 */
export const plainDescription = (html: string, maxLength: number = DESCRIPTION_MAX_LENGTH): string => {
  const text = textOf(parseRichText(html)).join(' ').replace(/\s+/g, ' ').trim()
  if (text.length <= maxLength) {
    return text
  }
  const cut = text.slice(0, maxLength - 1)
  const lastSpace = cut.lastIndexOf(' ')
  return `${(lastSpace > 0 ? cut.slice(0, lastSpace) : cut).trimEnd()}…`
}
