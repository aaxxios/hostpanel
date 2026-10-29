/**
 * Tests for `GET /api/portal/client/services/:id/ssh`: it reads the SSH access summary from the
 * signed-in `me/services/{id}/ssh` endpoint and forwards nothing but the service id.
 *
 * @module tests/server/api/portal/client/services/[id]/ssh/index.get.test
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
  route = await import('~/server/api/portal/client/services/[id]/ssh/index.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/client/services/:id/ssh', () => {
  it('returns the summary of the service ssh endpoint and forwards no query', async () => {
    backend.respondWith(() => ({ body: { host: 'srv.test', port: 22, username: 'acct' } }))

    const result = await route.default(makeEvent({ params: { id: '7' }, query: { host: 'other.test' }, cookies: await signedIn('jwt') }))

    expect(backend.requests[0]?.method).toBe('GET')
    expect(backend.requests[0]?.url).toBe('/api/me/services/7/ssh')
    expect(backend.requests[0]?.headers['authorization']).toBe('Bearer jwt')
    expect(result).toEqual({ host: 'srv.test', port: 22, username: 'acct' })
  })

  it('refuses a service id that is not an integer before calling the backend', async () => {
    const error = await refusal(async () => route.default(makeEvent({ params: { id: '7/../x' }, cookies: await signedIn('jwt') })))

    expect(error).toMatchObject({ statusCode: 400, data: { code: 'BAD_ROUTE_PARAM' } })
    expect(backend.requests).toHaveLength(0)
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 404, body: { type: 'SERVICE_NOT_FOUND', detail: 'Not found.' } }))

    const error = await refusal(async () => route.default(makeEvent({ params: { id: '7' }, cookies: await signedIn('jwt') })))

    expect(error).toMatchObject({ statusCode: 404, statusMessage: 'Not found.', data: { code: 'SERVICE_NOT_FOUND' } })
  })
})
