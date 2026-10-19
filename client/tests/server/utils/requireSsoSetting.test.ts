/**
 * Tests for `server/utils/requireSsoSetting.ts`: an empty single sign-on address is a coded 503.
 *
 * @module tests/server/utils/requireSsoSetting.test
 */

import { describe, expect, it } from 'vitest'
import { refusal } from '../../helpers/bffHarness'
import { requireSsoSetting, SSO_NOT_CONFIGURED } from '~/server/utils/requireSsoSetting'

describe('requireSsoSetting', () => {
  it('returns a configured address unchanged', () => {
    expect(requireSsoSetting('https://sso.test')).toBe('https://sso.test')
  })

  it.each([undefined, null, '', '   ', 5])('refuses %p with a coded 503', async (value) => {
    const error = await refusal(async () => requireSsoSetting(value))

    expect(error).toMatchObject({ statusCode: 503, data: { code: SSO_NOT_CONFIGURED } })
  })
})
