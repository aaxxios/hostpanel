/**
 * The one place that decides what the page says when the backend sends no name.
 *
 * The API sends `null` for a product name or a ticket author it could not resolve (a deleted
 * product, a removed account) and leaves the wording to the caller. Every screen goes through
 * these helpers so the fallback sentence is identical everywhere and lives in the locale files.
 *
 * @module utils/displayFallback
 */

import type { Translator } from '~/types/translator'

/**
 * Returns the trimmed name, or null when the backend sent nothing readable.
 *
 * @param name - A name from the API; null, undefined or blank means unknown.
 * @returns The trimmed text, or null.
 */
const readable = (name: string | null | undefined): string | null => {
  const trimmed = name?.trim()
  return trimmed ? trimmed : null
}

/**
 * The text for a name the backend did not send.
 *
 * @param t - Message lookup.
 * @param name - The name from the API, possibly null or blank.
 * @returns The name, or the localised "Unknown".
 */
export const nameOrUnknown = (t: Translator, name: string | null | undefined): string => readable(name) ?? t('fallback.unknown')

/**
 * The display name of a service's product.
 *
 * @param t - Message lookup.
 * @param productName - The product name from the API, possibly null or blank.
 * @param id - The service id, shown as "Product #id" when the name is missing.
 * @returns The product name, or the localised "Product #id".
 */
export const productNameOrFallback = (t: Translator, productName: string | null | undefined, id: number): string =>
  readable(productName) ?? t('fallback.product', { id })
