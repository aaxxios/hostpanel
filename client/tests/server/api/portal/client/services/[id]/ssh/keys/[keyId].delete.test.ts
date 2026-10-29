/**
 * Tests for `DELETE /api/portal/client/services/:id/ssh/keys/:keyId`: it revokes the key on the
 * signed-in backend endpoint, accepts only an integer service id and a GUID key id, and answers 204.
 *
 * @module tests/server/api/portal/client/services/[id]/ssh/keys/[keyId].delete.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend, signedIn } from '../../../../../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../../../../../helpers/bffHarness'
import type { StubServer } from '../../../../../../../../helpers/stubServer'
import { HttpMethod } from '~/types/httpmethod'

/** A key id in the GUID shape the backend route accepts. */
const KEY_ID = '3f2a1c4e-8b7d-4e6a-9c5b-1d2e3f4a5b6c'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/client/services/[id]/ssh/keys/[keyId].delete')
})

afterEach(async () => {
  await backend.close()
})

describe('DELETE /api/portal/client/services/:id/ssh/keys/:keyId', () => {
  it('deletes the key on the backend and answers 204 with no body', async () => {
    backend.respondWith(() => ({ status: 204 }))
    const event = makeEvent({ method: HttpMethod.Delete, params: { id: '7', keyId: KEY_ID }, cookies: await signedIn('jwt') })

    const result = await route.default(event)

    expect(backend.requests[0]?.method).toBe('DELETE')
    expect(backend.requests[0]?.url).toBe(`/api/me/services/7/ssh/keys/${KEY_ID}`)
    expect(event.status).toBe(204)
    expect(result).toBeNull()
  })

  it.each([
    { id: '7/x', keyId: KEY_ID },
    { id: '7', keyId: '../y' },
    { id: '7', keyId: 'abc' },
    { id: '7', keyId: `${KEY_ID}/x` }
  ])('refuses %j before calling the backend', async (params) => {
    const error = await refusal(async () => route.default(makeEvent({ method: HttpMethod.Delete, params, cookies: await signedIn('jwt') })))

    expect(error).toMatchObject({ statusCode: 400, data: { code: 'BAD_ROUTE_PARAM' } })
    expect(backend.requests).toHaveLength(0)
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 404, body: { type: 'SSH_KEY_NOT_FOUND', detail: 'Key not found.' } }))

    const error = await refusal(async () => route.default(makeEvent({ method: HttpMethod.Delete, params: { id: '7', keyId: KEY_ID }, cookies: await signedIn('jwt') })))

    expect(error).toMatchObject({ statusCode: 404, statusMessage: 'Key not found.', data: { code: 'SSH_KEY_NOT_FOUND' } })
  })
})
