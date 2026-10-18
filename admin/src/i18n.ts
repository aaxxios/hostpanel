/**
 * Admin panel i18n setup.
 *
 * Every shell and screen string is translated from `locales/<lang>/<area>.json` (assembled in
 * `locales/messages.ts`). Settings `key`/`description` values come from the backend as plain
 * strings — configuration identifiers and operator-facing copy stored in the database — so they
 * are not part of this catalogue.
 */
import { createI18n } from 'vue-i18n'
import { messages } from '@/locales/messages'

/** Locale codes the admin panel ships. */
export const SUPPORTED_LOCALES = ['en', 'hy', 'ru'] as const

/** One of the shipped locale codes. */
export type SupportedLocale = (typeof SUPPORTED_LOCALES)[number]

/** localStorage key holding the operator's chosen locale. */
const STORAGE_KEY = 'admin.locale'

/**
 * Reads the operator's saved locale preference, falling back to English for
 * an unseen visitor or a value the panel no longer ships.
 *
 * @returns A supported locale code.
 */
const initialLocale = (): SupportedLocale => {
  const stored = localStorage.getItem(STORAGE_KEY)
  if (stored && (SUPPORTED_LOCALES as readonly string[]).includes(stored)) {
    return stored as SupportedLocale
  }
  return 'en'
}

/** The application's vue-i18n instance (composition mode, English fallback). */
export const i18n = createI18n({
  legacy: false,
  locale: initialLocale(),
  fallbackLocale: 'en',
  messages,
})

/**
 * Switches the active locale and persists the choice.
 *
 * @param locale - The locale to switch to.
 */
export const setLocale = (locale: SupportedLocale): void => {
  i18n.global.locale.value = locale
  localStorage.setItem(STORAGE_KEY, locale)
}
