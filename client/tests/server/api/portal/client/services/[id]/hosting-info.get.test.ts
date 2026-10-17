/**
 * Tests for `GET /api/portal/client/services/:id/hosting-info`: it resolves the service from the
 * client's list. The name-server and certificate probes reach the public internet, so only the
 * paths that decide before probing are exercised here.
 *
 * @module tests/server/api/portal/client/services/[id]/hosting-info.get.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend } from '../../../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../../../helpers/bffHarness'
import type { StubServer } from '../../../../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/client/services/[id]/hosting-info.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/client/services/:id/hosting-info', () => {
  it('answers empty name servers and no certificate for a service without a domain, without probing', async () => {
    backend.respondWith(() => ({ body: [{ id: 7 }] }))

    const result = await route.default(makeEvent({ params: { id: '7' }, cookies: { auth_token: 'jwt' }, headers: { 'x-language': 'ru' } }))

    expect(result).toEqual({ nameservers: [], ssl: null })
    expect(backend.requests[0]?.url).toBe('/api/me/services')
    expect(backend.requests[0]?.headers['authorization']).toBe('Bearer jwt')
    expect(backend.requests[0]?.headers['x-language']).toBe('ru')
  })

  it('refuses with 404 and the not-found code when the service is not in the list', async () => {
    backend.respondWith(() => ({ body: [{ id: 3, domain: 'a.test' }] }))

    const error = await refusal(() => route.default(makeEvent({ params: { id: '7' }, cookies: { auth_token: 'jwt' } })))

    expect(error).toMatchObject({ statusCode: 404, data: { code: 'SERVICE_NOT_FOUND' } })
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 403, body: { type: 'FORBIDDEN', detail: 'No access.' } }))

    const error = await refusal(() => route.default(makeEvent({ params: { id: '7' }, cookies: { auth_token: 'jwt' } })))

    expect(error).toMatchObject({ statusCode: 403, statusMessage: 'No access.', data: { code: 'FORBIDDEN' } })
  })
})
