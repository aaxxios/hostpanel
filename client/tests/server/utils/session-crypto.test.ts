/**
 * Tests for `server/utils/session-crypto.ts`: the session round-trips through AES-GCM, a tampered
 * or foreign value reads as nothing, and the key is validated.
 *
 * @module tests/server/utils/session-crypto.test
 */

import { randomBytes } from 'node:crypto'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { decryptSession, encryptSession, readSessionKey } from '~/server/utils/session-crypto'

/** A valid key. */
const key = randomBytes(32)

afterEach(() => {
  vi.unstubAllEnvs()
})

describe('encryptSession and decryptSession', () => {
  it('round-trips a session and never stores the plain text', () => {
    const stored = encryptSession('{"accessToken":"at","refreshToken":"rt"}', key)

    expect(stored).toMatch(/^v1\./)
    expect(stored).not.toContain('refreshToken')
    expect(decryptSession(stored, key)).toBe('{"accessToken":"at","refreshToken":"rt"}')
  })

  it('uses a fresh IV for every write', () => {
    expect(encryptSession('x', key)).not.toBe(encryptSession('x', key))
  })

  it('reads a value written under another key, a tampered value and plain text as nothing', () => {
    const stored = encryptSession('secret', key)
    const bytes = Buffer.from(stored.slice(3), 'base64url')
    // Flip a bit inside the ciphertext so the authentication tag no longer matches.
    bytes[bytes.length - 1] = (bytes[bytes.length - 1] ?? 0) ^ 1
    const tampered = `v1.${bytes.toString('base64url')}`

    expect(decryptSession(stored, randomBytes(32))).toBeNull()
    expect(decryptSession(tampered, key)).toBeNull()
    expect(decryptSession('{"accessToken":"at"}', key)).toBeNull()
    expect(decryptSession('v1.short', key)).toBeNull()
  })
})

describe('readSessionKey', () => {
  it('decodes a 32-byte base64 key', () => {
    expect(readSessionKey(key.toString('base64'))?.equals(key)).toBe(true)
  })

  it('refuses a key of the wrong length', () => {
    expect(() => readSessionKey(randomBytes(16).toString('base64'))).toThrow('32 random bytes')
  })

  it('refuses a missing key outside development', () => {
    vi.stubEnv('NODE_ENV', 'production')

    expect(() => readSessionKey('')).toThrow('required')
    expect(() => readSessionKey(undefined)).toThrow('required')
  })

  it('allows a missing key in development', () => {
    vi.stubEnv('NODE_ENV', 'development')

    expect(readSessionKey('')).toBeNull()
  })
})
