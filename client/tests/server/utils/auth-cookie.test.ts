/**
 * Tests for the shared refresh-token cookie lifetime in `server/utils/auth-cookie.ts`.
 *
 * @module tests/server/utils/auth-cookie.test
 */

import { describe, expect, it } from 'vitest'
import { REFRESH_TOKEN_MAX_AGE, authCookieMaxAge } from '~/server/utils/auth-cookie'

describe('REFRESH_TOKEN_MAX_AGE', () => {
  it('is thirty days in seconds, the lifetime the identity provider grants a refresh token', () => {
    expect(REFRESH_TOKEN_MAX_AGE).toBe(30 * 24 * 60 * 60)
  })
})

describe('authCookieMaxAge', () => {
  /**
   * Builds an unsigned JWT-shaped string around a payload.
   *
   * @param payload - The claims.
   * @returns The compact token.
   */
  const tokenWith = (payload: Record<string, unknown>): string => {
    return `h.${Buffer.from(JSON.stringify(payload)).toString('base64url')}.s`
  }

  it('equals exp minus now', () => {
    expect(authCookieMaxAge(tokenWith({ exp: 1_000_900 }), 1_000_000)).toBe(900)
  })

  it('never goes negative for an expired token', () => {
    expect(authCookieMaxAge(tokenWith({ exp: 10 }), 1_000_000)).toBe(0)
  })

  it('gives a session cookie when exp is missing or the token is not a JWT', () => {
    expect(authCookieMaxAge(tokenWith({ sub: 'x' }), 1_000_000)).toBeUndefined()
    expect(authCookieMaxAge('opaque', 1_000_000)).toBeUndefined()
  })
})
