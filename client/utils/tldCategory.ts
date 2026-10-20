/**
 * Display label of a domain-extension category.
 *
 * @module utils/tldCategory
 */

import { TldCategory } from '~/types/tldcategory'

/** Translates a locale key; the `t` of vue-i18n satisfies it. */
export type CategoryTranslator = (key: string) => string

/**
 * The slug a registrar category is matched by: trimmed, lowercase, spaces as dashes.
 *
 * @param name - The category exactly as the registrar sent it.
 * @returns The slug, e.g. `real-estate`.
 */
export const tldCategorySlug = (name: string): string => name.trim().toLowerCase().replace(/\s+/g, '-')

/**
 * The label a visitor sees for a category: the translation of a known category, otherwise the
 * slug with dashes opened up and the first letter capitalised, so an operator's own category
 * never shows as a raw lowercase slug.
 *
 * @param name - The category exactly as the registrar sent it.
 * @param t - Translator of the active locale.
 * @returns The label, or an empty string for a blank name.
 */
export const tldCategoryLabel = (name: string, t: CategoryTranslator): string => {
  const slug = tldCategorySlug(name)
  if (!slug) {
    return ''
  }
  if ((Object.values(TldCategory) as string[]).includes(slug)) {
    return t(`domains.pricing.categories.${slug}`)
  }
  const words = slug.replace(/-/g, ' ')
  return words.charAt(0).toUpperCase() + words.slice(1)
}
