/**
 * Translation for code that runs outside a component (stores, utilities).
 *
 * `useI18n()` is a composable and cannot be called from a store action or a util. This helper
 * asks the Nuxt instance for its i18n plugin per call instead, the same way `formatDate.ts`
 * resolves its locale: nothing is cached at module level, so one Nitro process serving many
 * visitors never leaks one visitor's language into another's sentence.
 *
 * @module utils/translate
 */

/** The slice of the i18n plugin this helper needs. */
interface TranslatingNuxtApp {
  /** The `@nuxtjs/i18n` plugin instance, absent outside a running app. */
  $i18n?: {
    /** Resolves a message key in the active locale. */
    t: (key: string, named?: Record<string, unknown>) => string
  }
}

/**
 * Resolves a message key in the visitor's current language.
 *
 * @param key - Dotted key present in every locale catalogue.
 * @param named - Values for `{placeholders}` in the message.
 * @returns The translated sentence, or the key itself when there is no running Nuxt instance
 * (a unit test, or a call outside a render) so the caller still gets a non-empty string.
 */
export const translate = (key: string, named: Record<string, unknown> = {}): string => {
  try {
    const i18n = (useNuxtApp() as TranslatingNuxtApp).$i18n
    return i18n ? i18n.t(key, named) : key
  }
  catch {
    return key
  }
}
