/**
 * Unit tests for `constants/portalErrorCode.ts`: the codes are wire contract and must not drift.
 *
 * @module tests/constants/portalErrorCode.test
 */

import { describe, expect, it } from 'vitest'
import { PortalErrorCode } from '~/constants/portalErrorCode'

describe('PortalErrorCode', () => {
  it('keeps the wire values the API and the BFF send', () => {
    expect(PortalErrorCode.ClientProfileNotFound).toBe('CLIENT_PROFILE_NOT_FOUND')
    expect(PortalErrorCode.InviteSignInRequired).toBe('INVITE_SIGN_IN_REQUIRED')
  })

  it('writes every code in upper case', () => {
    for (const code of Object.values(PortalErrorCode)) {
      expect(code).toBe(code.toUpperCase())
    }
  })
})
