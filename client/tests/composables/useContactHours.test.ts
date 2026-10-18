/**
 * Tests for `useContactHours`: the operator's working hours and time zone, hidden when unset.
 *
 * @module tests/composables/useContactHours.test
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useContactHours } from '~/composables/useContactHours'

/**
 * Stubs the settings reader with a fixed set of values.
 *
 * @param values Setting key to value.
 */
const stubSettings = (values: Record<string, string>): void => {
  vi.stubGlobal('usePortalSettings', () => ({ get: (key: string) => values[key] ?? '' }))
}

describe('useContactHours', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('joins the hours and the time zone the operator set', () => {
    stubSettings({ 'portal.contact.hours': '08:00 - 17:00', 'portal.contact.timezone': 'UTC+2' })

    expect(useContactHours().workingHours.value).toBe('08:00 - 17:00 UTC+2')
  })

  it('shows the hours alone when no time zone is set', () => {
    stubSettings({ 'portal.contact.hours': '08:00 - 17:00' })

    expect(useContactHours().workingHours.value).toBe('08:00 - 17:00')
  })

  it('is empty when no hours are set, even with a time zone', () => {
    stubSettings({ 'portal.contact.timezone': 'UTC+2' })

    expect(useContactHours().workingHours.value).toBe('')
  })
})
