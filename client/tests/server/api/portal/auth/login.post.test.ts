/**
 * Tests for `POST /api/portal/auth/login`: the sign-in route that trades credentials for the
 * httpOnly session cookies, or passes a two-factor challenge through untouched.
 *
 * The backend is a real HTTP stub; the assertions are on what it received (anonymous call, the
 * reader's language) and on the cookies the route set.
 *
 * @module tests/server/api/portal/auth/login.post.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend } from '../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../helpers/bffHarness'
import type { StubServer } from '../../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/auth/login.post')
})

afterEach(async () => {
  await backend.close()
})

describe('POST /api/portal/auth/login', () => {
  it('stores the access token in an httpOnly cookie sized by expiresIn and answers success', async () => {
    backend.respondWith(() => ({ body: { accessToken: 'jwt-1', expiresIn: 600 } }))
    const event = makeEvent({ body: { email: 'a@b.test', password: 'pw' }, headers: { 'x-language': 'hy' } })

    const result = await route.default(event)

    expect(result).toEqual({ success: true })
    expect(event.written['auth_token']).toEqual({
      value: 'jwt-1',
      options: expect.objectContaining({ httpOnly: true, maxAge: 600, sameSite: 'lax', path: '/' }),
    })
    expect(event.written['authed']?.options).toMatchObject({ httpOnly: false })
  })

  it('calls the backend anonymously with the body and the reader language', async () => {
    backend.respondWith(() => ({ body: { accessToken: 'jwt-1', expiresIn: 600 } }))

    await route.default(makeEvent({
      body: { email: 'a@b.test', password: 'pw' },
      headers: { 'x-language': 'ru' },
      cookies: { auth_token: 'stale' },
    }))

    expect(backend.requests).toHaveLength(1)
    expect(backend.requests[0]?.url).toBe('/api/auth/login')
    expect(backend.requests[0]?.method).toBe('POST')
    expect(backend.requests[0]?.headers['authorization']).toBeUndefined()
    expect(backend.requests[0]?.headers['x-language']).toBe('ru')
    expect(JSON.parse(backend.requests[0]?.body ?? '{}')).toEqual({ email: 'a@b.test', password: 'pw' })
  })

  it('passes a two-factor challenge through and sets no cookie', async () => {
    backend.respondWith(() => ({ body: { twoFactorRequired: true, pendingToken: 'pending-1' } }))
    const event = makeEvent({ body: { email: 'a@b.test', password: 'pw' } })

    const result = await route.default(event)

    expect(result).toEqual({ twoFactorRequired: true, pendingToken: 'pending-1' })
    expect(event.written).toEqual({})
  })

  it('rethrows the backend refusal with its sentence and code and sets no cookie', async () => {
    backend.respondWith(() => ({ status: 401, body: { type: 'INVALID_CREDENTIALS', detail: 'Wrong password.' } }))
    const event = makeEvent({ body: { email: 'a@b.test', password: 'bad' } })

    const error = await refusal(() => route.default(event))

    expect(error).toMatchObject({ statusCode: 401, statusMessage: 'Wrong password.', data: { code: 'INVALID_CREDENTIALS' } })
    expect(event.written).toEqual({})
  })
})
