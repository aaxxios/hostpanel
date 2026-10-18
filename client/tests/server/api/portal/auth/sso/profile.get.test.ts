/**
 * Tests for `GET /api/portal/auth/sso/profile`: it reads the SSO account profile with the session
 * token, makes sure the subject id is present, and adds the portal menu.
 *
 * @module tests/server/api/portal/auth/sso/profile.get.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend } from '../../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../../helpers/bffHarness'
import type { StubServer } from '../../../../../helpers/stubServer'

/** The stub playing the SSO profile endpoint. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/auth/sso/profile.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/auth/sso/profile', () => {
  it('returns the profile with the subject and the menu, calling the SSO with the bearer', async () => {
    backend.respondWith(() => ({ body: { sub: 'u-1', email: 'a@b.test' } }))

    const result = await route.default(makeEvent({ cookies: { auth_token: 'jwt' } })) as Record<string, unknown>

    expect(backend.requests[0]?.url).toBe('/api/account/profile')
    expect(backend.requests[0]?.headers['authorization']).toBe('Bearer jwt')
    expect(result).toMatchObject({ sub: 'u-1', ssoSub: 'u-1', email: 'a@b.test' })
    expect((result['menuItems'] as { url: string }[]).map(item => item.url)).toContain('/client/dashboard')
  })

  it('reads the subject from the token when the profile omits it', async () => {
    backend.respondWith(() => ({ body: { email: 'a@b.test' } }))
    const payload = Buffer.from(JSON.stringify({ sub: 'from-token' })).toString('base64url')

    const result = await route.default(makeEvent({ cookies: { auth_token: `h.${payload}.s` } })) as Record<string, unknown>

    expect(result['ssoSub']).toBe('from-token')
  })

  it('answers with an empty subject when neither the profile nor the token has one', async () => {
    backend.respondWith(() => ({ body: {} }))

    const result = await route.default(makeEvent({ cookies: { auth_token: 'not-a-jwt' } })) as Record<string, unknown>

    expect(result['ssoSub']).toBe('')
  })

  it('refuses with 401 and calls nothing when there is no session', async () => {
    const error = await refusal(() => route.default(makeEvent()))

    expect(error).toMatchObject({ statusCode: 401 })
    expect(backend.requests).toHaveLength(0)
  })
})
