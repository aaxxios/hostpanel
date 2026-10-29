/**
 * Unit tests for `utils/emailPreferences.ts`: picking the editable categories out of the profile's
 * notification flags.
 */
import { describe, expect, it } from 'vitest'
import { decodeEmailPrefs } from '~/utils/emailPreferences'
import type { MyProfileNotifications } from '~/types/myprofilenotifications'

/** Notification flags as the backend sends them, two categories switched off. */
const notifications: MyProfileNotifications = {
  general: false,
  invoice: true,
  support: true,
  product: false,
  domain: true,
  affiliate: true,
  marketingOptIn: false,
  statusUpdate: true
}

describe('decodeEmailPrefs', () => {
  it('treats missing flags as every category on', () => {
    for (const prefs of [undefined, null]) {
      const flags = decodeEmailPrefs(prefs)

      expect(Object.values(flags).every(Boolean)).toBe(true)
      expect(Object.keys(flags)).toHaveLength(6)
    }
  })

  it('keeps the six editable categories as the backend sent them', () => {
    const flags = decodeEmailPrefs(notifications)

    expect(flags).toEqual({ general: false, invoice: true, support: true, product: false, domain: true, affiliate: true })
  })
})
