/**
 * Translation for code that runs outside a component (stores, utilities).
 *
 * A component asks vue-i18n for words through `useI18n()`. A store has no component
 * instance, so it reaches the same message catalogue through the app's i18n instance.
 * The key is resolved at call time, never at module load, so a language switch is honoured.
 */

import { i18n } from '@/i18n'

/**
 * Resolves a message key in the operator's current language.
 *
 * @param key - Dotted key present in every locale catalogue.
 * @param named - Values for `{placeholders}` in the message.
 * @returns The translated sentence.
 */
export const translate = (key: string, named: Record<string, unknown> = {}): string =>
  i18n.global.t(key, named)

/**
 * Resolves a message key when the catalogue has it, otherwise returns the raw value.
 *
 * Used for values the API sends as identifiers (a ticket status, say) that may or may not
 * have a translation yet: an unknown value is shown as received instead of as a bare key.
 *
 * @param key - Dotted key to look up.
 * @param raw - What to show when the key is not in the catalogue.
 * @returns The translated sentence, or `raw`.
 */
export const translateOrRaw = (key: string, raw: string): string =>
  i18n.global.te(key) ? i18n.global.t(key) : raw
