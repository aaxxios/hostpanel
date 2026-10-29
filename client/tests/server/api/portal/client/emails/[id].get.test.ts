/**
 * Tests for `GET /api/portal/client/emails/:id`: it calls the backend `GET /me/emails/7` with the session bearer, forwards the reader language and returns the backend message as sent.
 *
 * @module tests/server/api/portal/client/emails/[id].get.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend, signedIn } from '../../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../../helpers/bffHarness'
import type { StubServer } from '../../../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/client/emails/[id].get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/client/emails/:id', () => {
  it('calls the backend endpoint and passes its answer through', async () => {
    const message = { id: 7, to: 'a@b.test', subject: 'Invoice', body: '<p>Hello</p>', sentAt: '2026-02-03T00:00:00Z', success: true }
    backend.respondWith(() => ({ body: message }))

    const result = await route.default(makeEvent({ params: { id: '7' }, cookies: await signedIn('jwt') }))

    expect(result).toEqual(message)
    expect(backend.requests[0]?.method).toBe('GET')
    expect(backend.requests[0]?.url).toBe('/api/me/emails/7')
    expect(backend.requests[0]?.headers['authorization']).toBe('Bearer jwt')
  })

  it('forwards the reader language to the backend', async () => {
    backend.respondWith(() => ({ body: { id: 7, to: 'a@b.test', subject: 'Invoice', body: '', sentAt: '2026-02-03T00:00:00Z', success: true } }))
    const event = makeEvent({ params: { id: '7' }, headers: { 'x-language': 'hy' }, cookies: await signedIn('jwt') })

    await route.default(event)

    expect(backend.requests[0]?.headers['x-language']).toBe('hy')
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 409, body: { type: 'SOME_REFUSAL', detail: 'Refused by the backend.' } }))

    const error = await refusal(async () => route.default(makeEvent({ params: { id: '7' }, cookies: await signedIn('jwt') })))

    expect(error).toMatchObject({ statusCode: 409, statusMessage: 'Refused by the backend.', data: { code: 'SOME_REFUSAL' } })
  })
})
