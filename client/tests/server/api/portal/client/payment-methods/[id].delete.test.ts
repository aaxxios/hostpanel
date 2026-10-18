/**
 * Tests for `DELETE /api/portal/client/payment-methods/:id`: it calls the backend `DELETE /me/payment-methods/7` with the session bearer, forwards the reader language and returns the backend's answer untouched.
 *
 * @module tests/server/api/portal/client/payment-methods/[id].delete.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend } from '../../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../../helpers/bffHarness'
import type { StubServer } from '../../../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/client/payment-methods/[id].delete')
})

afterEach(async () => {
  await backend.close()
})

describe('DELETE /api/portal/client/payment-methods/:id', () => {
  it('calls the backend endpoint and returns its answer', async () => {
    backend.respondWith(() => ({ body: { ok: true } }))

    const result = await route.default(makeEvent({ params: { id: '7' }, cookies: { auth_token: 'jwt' } }))

    expect(result).toEqual({ ok: true })
    expect(backend.requests[0]?.method).toBe('DELETE')
    expect(backend.requests[0]?.url).toBe('/api/me/payment-methods/7')
    expect(backend.requests[0]?.headers['authorization']).toBe('Bearer jwt')
  })

  it('forwards the reader language to the backend', async () => {
    const event = makeEvent({ params: { id: '7' }, headers: { 'x-language': 'hy' }, cookies: { auth_token: 'jwt' } })

    await route.default(event)

    expect(backend.requests[0]?.headers['x-language']).toBe('hy')
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 409, body: { type: 'SOME_REFUSAL', detail: 'Refused by the backend.' } }))

    const error = await refusal(() => route.default(makeEvent({ params: { id: '7' }, cookies: { auth_token: 'jwt' } })))

    expect(error).toMatchObject({ statusCode: 409, statusMessage: 'Refused by the backend.', data: { code: 'SOME_REFUSAL' } })
  })
})
