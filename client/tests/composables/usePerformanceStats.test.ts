/**
 * Tests for `usePerformanceStats`: only set figures, in enum order.
 *
 * @module tests/composables/usePerformanceStats.test
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { PerformanceStatKey } from '~/constants/performanceStatKey'
import { PortalSettingKey } from '~/constants/portalSettingKey'
import { usePerformanceStats } from '~/composables/usePerformanceStats'

/**
 * Stubs the settings reader with a fixed set of values.
 *
 * @param values Setting key to value.
 */
const stubSettings = (values: Record<string, string>): void => {
  vi.stubGlobal('usePortalSettings', () => ({ get: (key: string) => values[key] ?? '' }))
}

describe('usePerformanceStats', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('is empty when no figure is set, which hides the section', () => {
    stubSettings({})

    expect(usePerformanceStats().value).toEqual([])
  })

  it('returns the set figures in enum order', () => {
    stubSettings({
      [PortalSettingKey.StatsSupportResponse]: '15 min',
      [PortalSettingKey.StatsUptime]: '99.9%',
      [PortalSettingKey.StatsSitesHosted]: '12,000+',
    })

    expect(usePerformanceStats().value).toEqual([
      { key: PerformanceStatKey.Uptime, value: '99.9%' },
      { key: PerformanceStatKey.SitesHosted, value: '12,000+' },
      { key: PerformanceStatKey.SupportResponse, value: '15 min' },
    ])
  })
})
