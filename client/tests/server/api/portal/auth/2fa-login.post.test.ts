/**
 * Tests for `POST /api/portal/auth/2fa-login`: completing a two-factor sign-in with the pending
 * token and the authenticator code.
 *
 * @module tests/server/api/portal/auth/2fa-login.post.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend, storedSession } from '../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../helpers/bffHarness'
import type { StubServer } from '../../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/auth/2fa-login.post')
})

afterEach(async () => {
  await backend.close()
})

describe('POST /api/portal/auth/2fa-login', () => {
  it('forwards pending token and code anonymously, then stores the session cookies', async () => {
    backend.respondWith(() => ({ body: { accessToken: 'jwt-2', expiresIn: 900 } }))
    const event = makeEvent({ body: { pendingToken: 'p', code: '123456' }, headers: { 'x-language': 'ru' } })

    const result = await route.default(event)

    expect(result).toEqual({ success: true })
    expect(backend.requests[0]?.url).toBe('/api/auth/2fa-login')
    expect(backend.requests[0]?.headers['authorization']).toBeUndefined()
    expect(backend.requests[0]?.headers['x-language']).toBe('ru')
    expect(JSON.parse(backend.requests[0]?.body ?? '{}')).toEqual({ pendingToken: 'p', code: '123456' })
    expect(await storedSession({ portal_session: event.written['portal_session']?.value ?? '' })).toMatchObject({ accessToken: 'jwt-2' })
    expect(event.written['auth_token']).toBeUndefined()
    expect(event.written['authed']?.value).toBe('1')
  })

  it('sets no cookie when the code is refused', async () => {
    backend.respondWith(() => ({ status: 400, body: { type: 'INVALID_CODE', detail: 'Wrong code.' } }))
    const event = makeEvent({ body: { pendingToken: 'p', code: '000000' } })

    const error = await refusal(() => route.default(event))

    expect(error).toMatchObject({ statusCode: 400, data: { code: 'INVALID_CODE' } })
    expect(event.written).toEqual({})
  })
})
