/**
 * Tests for `useGuarantees`: each promise shows only when set, the money-back one with its days.
 *
 * @module tests/composables/useGuarantees.test
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { GuaranteeKey } from '~/constants/guaranteeKey'
import { PortalSettingKey } from '~/constants/portalSettingKey'
import { useGuarantees } from '~/composables/useGuarantees'

/**
 * Stubs the settings reader with a fixed set of values.
 *
 * @param values Setting key to value.
 */
const stubSettings = (values: Record<string, string>): void => {
  vi.stubGlobal('usePortalSettings', () => ({ get: (key: string) => values[key] ?? '' }))
}

describe('useGuarantees', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('is empty when nothing is set', () => {
    stubSettings({})

    expect(useGuarantees().value).toEqual([])
  })

  it('returns every promise in enum order, money back with its days', () => {
    stubSettings({
      [PortalSettingKey.GuaranteeNoHiddenFees]: 'true',
      [PortalSettingKey.GuaranteeMoneyBackDays]: '30',
      [PortalSettingKey.MigrationFree]: 'true',
    })

    expect(useGuarantees().value).toEqual([
      { key: GuaranteeKey.FreeMigration },
      { key: GuaranteeKey.MoneyBack, days: 30 },
      { key: GuaranteeKey.NoHiddenFees },
    ])
  })

  it('ignores a money-back period that is zero or not digits', () => {
    stubSettings({ [PortalSettingKey.GuaranteeMoneyBackDays]: '0' })
    expect(useGuarantees().value).toEqual([])

    stubSettings({ [PortalSettingKey.GuaranteeMoneyBackDays]: 'thirty' })
    expect(useGuarantees().value).toEqual([])
  })
})
