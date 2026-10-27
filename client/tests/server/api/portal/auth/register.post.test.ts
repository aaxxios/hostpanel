/**
 * Tests for `POST /api/portal/auth/register`: it calls the backend `POST /auth/register` without a bearer token, forwards the reader language and returns the backend's answer untouched.
 *
 * @module tests/server/api/portal/auth/register.post.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend, signedIn } from '../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../helpers/bffHarness'
import type { StubServer } from '../../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/auth/register.post')
})

afterEach(async () => {
  await backend.close()
})

describe('POST /api/portal/auth/register', () => {
  it('calls the backend endpoint and returns its answer', async () => {
    backend.respondWith(() => ({ body: { ok: true } }))

    const result = await route.default(makeEvent({ body: { value: 'x' }, cookies: await signedIn('jwt') }))

    expect(result).toEqual({ ok: true })
    expect(backend.requests[0]?.method).toBe('POST')
    expect(backend.requests[0]?.url).toBe('/api/auth/register')
    expect(backend.requests[0]?.headers['authorization']).toBeUndefined()
    expect(JSON.parse(backend.requests[0]?.body ?? '{}')).toEqual({ value: 'x' })
  })

  it('forwards the reader language to the backend', async () => {
    const event = makeEvent({ body: { value: 'x' }, headers: { 'x-language': 'hy' }, cookies: await signedIn('jwt') })

    await route.default(event)

    expect(backend.requests[0]?.headers['x-language']).toBe('hy')
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 409, body: { type: 'SOME_REFUSAL', detail: 'Refused by the backend.' } }))

    const error = await refusal(async () => route.default(makeEvent({ body: { value: 'x' }, cookies: await signedIn('jwt') })))

    expect(error).toMatchObject({ statusCode: 409, statusMessage: 'Refused by the backend.', data: { code: 'SOME_REFUSAL' } })
  })
})
