/**
 * Decoding of the profile's email preferences.
 *
 * @module utils/emailPreferences
 */

import { EmailPreference } from '~/types/emailpreference'

/** The preferences as the page edits them: one boolean per category. */
export type EmailPreferenceFlags = Record<EmailPreference, boolean>

/**
 * Turns the API's preferences into booleans.
 *
 * The legacy API may return a value as a string ("1"/"0") or a number (1/0), and omits the whole
 * object for a fresh account; `Number()` handles both spellings and a missing flag counts as on.
 *
 * @param prefs - The profile's `email_preferences`, or undefined.
 * @returns One boolean per category.
 */
export const decodeEmailPrefs = (prefs: Partial<Record<EmailPreference, unknown>> | undefined): EmailPreferenceFlags => {
  const flags = {} as EmailPreferenceFlags
  for (const key of Object.values(EmailPreference)) {
    flags[key] = Number(prefs?.[key] ?? 1) !== 0
  }
  return flags
}
