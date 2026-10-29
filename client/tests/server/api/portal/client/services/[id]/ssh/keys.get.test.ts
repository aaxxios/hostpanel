/**
 * Tests for `GET /api/portal/client/services/:id/ssh/keys`: it lists the SSH keys from the
 * signed-in `me/services/{id}/ssh/keys` endpoint.
 *
 * @module tests/server/api/portal/client/services/[id]/ssh/keys.get.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend, signedIn } from '../../../../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../../../../helpers/bffHarness'
import type { StubServer } from '../../../../../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/client/services/[id]/ssh/keys.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/client/services/:id/ssh/keys', () => {
  it('returns the key list of the service', async () => {
    backend.respondWith(() => ({ body: [{ id: 'k1', label: 'laptop' }] }))

    const result = await route.default(makeEvent({ params: { id: '7' }, cookies: await signedIn('jwt') }))

    expect(backend.requests[0]?.method).toBe('GET')
    expect(backend.requests[0]?.url).toBe('/api/me/services/7/ssh/keys')
    expect(backend.requests[0]?.headers['authorization']).toBe('Bearer jwt')
    expect(result).toEqual([{ id: 'k1', label: 'laptop' }])
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 404, body: { type: 'SERVICE_NOT_FOUND', detail: 'Not found.' } }))

    const error = await refusal(async () => route.default(makeEvent({ params: { id: '7' }, cookies: await signedIn('jwt') })))

    expect(error).toMatchObject({ statusCode: 404, statusMessage: 'Not found.', data: { code: 'SERVICE_NOT_FOUND' } })
  })
})
