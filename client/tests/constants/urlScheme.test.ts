/**
 * Unit tests for `constants/urlScheme.ts` and `constants/settingValue.ts`.
 *
 * @module tests/constants/urlScheme.test
 */

import { describe, expect, it } from 'vitest'
import { SETTING_ON } from '~/constants/settingValue'
import { HTTPS_PREFIX } from '~/constants/urlScheme'

describe('scheme and setting constants', () => {
  it('keeps the secure scheme prefix', () => {
    expect(HTTPS_PREFIX).toBe('https://')
  })

  it('keeps the stored on value', () => {
    expect(SETTING_ON).toBe('true')
  })
})
