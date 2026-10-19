/**
 * A message lookup function.
 *
 * @module types/translator
 */

/** A message lookup, as `useI18n().t` or the template's `$t` provides it. */
export type Translator = (key: string, named?: Record<string, unknown>) => string
