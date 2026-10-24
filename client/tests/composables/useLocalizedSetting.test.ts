/**
 * Tests for `useLocalizedSetting`: the per-language row when the visitor's language has one,
 * the default row otherwise.
 *
 * @module tests/composables/useLocalizedSetting.test
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { useLocalizedSetting } from '~/composables/useLocalizedSetting'
import type { LocalizedSettingSpec } from '~/composables/useLocalizedSetting'
import { PortalSettingKey } from '~/constants/portalSettingKey'

/** The office address rows, used as the sample setting. */
const ADDRESS: LocalizedSettingSpec = {
  default: { key: PortalSettingKey.ContactAddress, envKey: 'portalContactAddress' },
  ru: { key: PortalSettingKey.ContactAddressRu, envKey: 'portalContactAddressRu' },
  hy: { key: PortalSettingKey.ContactAddressHy, envKey: 'portalContactAddressHy' }
}

/**
 * Stubs the settings reader and the active language.
 *
 * @param values Setting key to value.
 * @param locale The visitor's language.
 */
const stub = (values: Record<string, string>, locale: string): void => {
  vi.stubGlobal('usePortalSettings', () => ({ get: (key: string) => values[key] ?? '' }))
  vi.stubGlobal('useI18n', () => ({ locale: ref(locale) }))
}

describe('useLocalizedSetting', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('reads the default row for English', () => {
    stub({ 'portal.contact.address': 'Default', 'portal.contact.address.ru': 'Русский' }, 'en')

    expect(useLocalizedSetting().getLocalized(ADDRESS)).toBe('Default')
  })

  it('reads the Russian row for a Russian visitor', () => {
    stub({ 'portal.contact.address': 'Default', 'portal.contact.address.ru': 'Русский' }, 'ru')

    expect(useLocalizedSetting().getLocalized(ADDRESS)).toBe('Русский')
  })

  it('reads the Armenian row for an Armenian visitor', () => {
    stub({ 'portal.contact.address': 'Default', 'portal.contact.address.hy': 'Հայերեն' }, 'hy')

    expect(useLocalizedSetting().getLocalized(ADDRESS)).toBe('Հայերեն')
  })

  it('falls back to the default row when the language has none', () => {
    stub({ 'portal.contact.address': 'Default' }, 'ru')

    expect(useLocalizedSetting().getLocalized(ADDRESS)).toBe('Default')
  })

  it('is empty when nothing is set', () => {
    stub({}, 'hy')

    expect(useLocalizedSetting().getLocalized(ADDRESS)).toBe('')
  })
})
