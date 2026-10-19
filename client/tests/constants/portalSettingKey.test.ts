/**
 * Unit tests for `constants/portalSettingKey.ts`: every key is unique and lives in the
 * `portal.` namespace.
 *
 * @module tests/constants/portalSettingKey
 */

import { describe, expect, it } from 'vitest'
import { PortalSettingKey } from '~/constants/portalSettingKey'

/** Every setting key the enum declares. */
const keys = Object.values(PortalSettingKey) as string[]

describe('PortalSettingKey', () => {
  it('has unique values', () => {
    expect(new Set(keys).size).toBe(keys.length)
  })

  it('starts every value with the portal. namespace', () => {
    for (const key of keys) {
      expect(key.startsWith('portal.')).toBe(true)
    }
  })
})
