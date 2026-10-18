/**
 * Tests for `GET /api/portal/auth/email-verified`: it reads the backend `GET /auth/email-verified` with the session bearer and returns the three-valued answer untouched.
 *
 * @module tests/server/api/portal/auth/email-verified.get.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, startBackend } from '../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../helpers/bffHarness'
import type { StubServer } from '../../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/auth/email-verified.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/auth/email-verified', () => {
  it('calls the backend endpoint and returns its answer', async () => {
    backend.respondWith(() => ({ body: { verified: false } }))

    const result = await route.default(makeEvent({ cookies: { auth_token: 'jwt' } }))

    expect(result).toEqual({ verified: false })
    expect(backend.requests[0]?.method).toBe('GET')
    expect(backend.requests[0]?.url).toBe('/api/auth/email-verified')
    expect(backend.requests[0]?.headers['authorization']).toBe('Bearer jwt')
  })

  it('keeps a null answer null', async () => {
    backend.respondWith(() => ({ body: { verified: null } }))

    const result = await route.default(makeEvent({ cookies: { auth_token: 'jwt' } }))

    expect(result).toEqual({ verified: null })
  })
})
