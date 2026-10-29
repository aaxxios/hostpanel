/**
 * Tests for `POST /api/portal/client/email-change`: it calls the backend `POST /api/clients/me/email-change` with the session
 * bearer, forwards the reader language and passes the body and refusals through.
 *
 * @module tests/server/api/portal/client/email-change.post.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend, signedIn } from '../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../helpers/bffHarness'
import type { StubServer } from '../../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

/** The body the browser sends. */
const body = { newEmail: 'new@example.com', currentPassword: 'pw' }

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/client/email-change.post')
})

afterEach(async () => {
  await backend.close()
})

describe('POST /api/portal/client/email-change', () => {
  it('calls the backend endpoint with the bearer and the body as is', async () => {
    backend.respondWith(() => ({ body: {} }))

    await route.default(makeEvent({ body, cookies: await signedIn('jwt') }))

    expect(backend.requests[0]?.method).toBe('POST')
    expect(backend.requests[0]?.url).toBe('/api/clients/me/email-change')
    expect(backend.requests[0]?.headers['authorization']).toBe('Bearer jwt')
    expect(JSON.parse(backend.requests[0]?.body ?? '{}')).toEqual(body)
  })

  it('forwards the reader language to the backend', async () => {
    backend.respondWith(() => ({ body: {} }))

    await route.default(makeEvent({ body, headers: { 'x-language': 'hy' }, cookies: await signedIn('jwt') }))

    expect(backend.requests[0]?.headers['x-language']).toBe('hy')
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 409, body: { type: 'SOME_REFUSAL', detail: 'Refused by the backend.' } }))

    const error = await refusal(async () => route.default(makeEvent({ body, cookies: await signedIn('jwt') })))

    expect(error).toMatchObject({ statusCode: 409, statusMessage: 'Refused by the backend.', data: { code: 'SOME_REFUSAL' } })
  })
})
