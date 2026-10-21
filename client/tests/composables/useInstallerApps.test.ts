/**
 * Tests for `useInstallerApps`: the count comes from the setting, the categories are fixed.
 *
 * @module tests/composables/useInstallerApps.test
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { InstallerAppCategory } from '~/constants/installerAppCategory'
import { PortalSettingKey } from '~/constants/portalSettingKey'
import { useInstallerApps } from '~/composables/useInstallerApps'

/**
 * Stubs the settings reader with a fixed set of values.
 *
 * @param values Setting key to value.
 */
const stubSettings = (values: Record<string, string>): void => {
  vi.stubGlobal('usePortalSettings', () => ({ get: (key: string) => values[key] ?? '' }))
}

describe('useInstallerApps', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('reads the count as the operator wrote it', () => {
    stubSettings({ [PortalSettingKey.InstallerAppCount]: '150+' })

    expect(useInstallerApps().value.count).toBe('150+')
  })

  it('has an empty count when unset, but still every category', () => {
    stubSettings({})

    const apps = useInstallerApps().value

    expect(apps.count).toBe('')
    expect(apps.categories).toEqual(Object.values(InstallerAppCategory))
  })
})
