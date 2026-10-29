/**
 * Decoding of the profile's email preferences.
 *
 * @module utils/emailPreferences
 */

import { EmailPreference } from '~/types/emailpreference'
import type { MyProfileNotifications } from '~/types/myprofilenotifications'

/** The preferences as the page edits them: one boolean per category. */
export type EmailPreferenceFlags = Record<EmailPreference, boolean>

/**
 * Picks the editable categories out of the profile's notification flags.
 *
 * The profile has no flags at all for an identity without a client record (and none before the
 * profile has loaded); every category then reads as on, the default a new account starts with.
 *
 * @param prefs - The profile's `notifications`, or null/undefined when there are none.
 * @returns One boolean per category.
 */
export const decodeEmailPrefs = (prefs: MyProfileNotifications | null | undefined): EmailPreferenceFlags => {
  const flags = {} as EmailPreferenceFlags
  for (const key of Object.values(EmailPreference)) {
    flags[key] = prefs?.[key] ?? true
  }
  return flags
}
