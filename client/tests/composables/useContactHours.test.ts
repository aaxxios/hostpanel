/**
 * Tests for `useContactHours`: the operator's working hours and time zone, hidden when unset.
 *
 * @module tests/composables/useContactHours.test
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { useContactHours } from '~/composables/useContactHours'

/**
 * Stubs the settings reader with a fixed set of values.
 *
 * @param values Setting key to value.
 * @param locale The visitor's language.
 */
const stubSettings = (values: Record<string, string>, locale = 'en'): void => {
  vi.stubGlobal('usePortalSettings', () => ({ get: (key: string) => values[key] ?? '' }))
  vi.stubGlobal('useI18n', () => ({ locale: ref(locale) }))
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

  it('uses the hours written for the visitor language, with the one time zone', () => {
    stubSettings({ 'portal.contact.hours': 'Mon-Fri', 'portal.contact.hours.ru': 'Пн-Пт', 'portal.contact.timezone': 'UTC+4' }, 'ru')

    expect(useContactHours().workingHours.value).toBe('Пн-Пт UTC+4')
  })

  it('falls back to the default hours when the language has none', () => {
    stubSettings({ 'portal.contact.hours': 'Mon-Fri' }, 'hy')

    expect(useContactHours().workingHours.value).toBe('Mon-Fri')
  })
})
