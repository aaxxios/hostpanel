/**
 * Knowledgebase category slugs: the one place a free-text category name becomes a URL segment and
 * a safe test id part.
 *
 * Category names are operator text (`Getting Started`, `Billing & Payments`), so they can hold
 * spaces and punctuation that make a poor URL or `data-testid`. The category page route and the
 * test ids use the slug; the content store resolves a slug back to its category.
 *
 * @module utils/kbCategory
 */

/**
 * The lowercase slug of a category name: every run of characters that is not a letter or a digit
 * becomes one dash, with no dash at either end. Letters of any script are kept.
 *
 * @param name - The category name as the backend sends it.
 * @returns The slug, e.g. `billing-payments` for `Billing & Payments`.
 */
export const kbCategorySlug = (name: string): string =>
  name
    .trim()
    .toLowerCase()
    .replace(/[^\p{L}\p{N}]+/gu, '-')
    .replace(/^-+|-+$/g, '')
