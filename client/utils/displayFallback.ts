/**
 * The one place that decides what the page says when the backend sends no name.
 *
 * The API sends an empty or `null` ticket author when the reply has none (a removed account) and
 * leaves the wording to the caller. Every screen goes through this helper so the fallback sentence
 * is identical everywhere and lives in the locale files.
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
