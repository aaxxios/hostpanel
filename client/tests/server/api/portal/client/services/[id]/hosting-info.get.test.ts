/**
 * Tests for `GET /api/portal/client/services/:id/hosting-info`: a pass-through to the backend read
 * of the same name. The route resolves nothing and opens no connection itself.
 *
 * @module tests/server/api/portal/client/services/[id]/hosting-info.get.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend, signedIn } from '../../../../../../helpers/bffHarness'
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
  it('returns the backend reading untouched and forwards the bearer and the reader language', async () => {
    const reading = {
      nameservers: ['ns1.example.net'],
      ssl: { valid: true, issuer: 'Some CA', startDate: '2026-01-01T00:00:00+00:00', expiryDate: '2027-01-01T00:00:00+00:00' }
    }
    backend.respondWith(() => ({ body: reading }))

    const result = await route.default(makeEvent({ params: { id: '7' }, cookies: await signedIn('jwt'), headers: { 'x-language': 'ru' } }))

    expect(result).toEqual(reading)
    expect(backend.requests).toHaveLength(1)
    expect(backend.requests[0]?.method).toBe('GET')
    expect(backend.requests[0]?.url).toBe('/api/me/services/7/hosting-info')
    expect(backend.requests[0]?.headers['authorization']).toBe('Bearer jwt')
    expect(backend.requests[0]?.headers['x-language']).toBe('ru')
  })

  it('does not load the services list or look at any domain itself', async () => {
    backend.respondWith(() => ({ body: { nameservers: [], ssl: null } }))

    await route.default(makeEvent({ params: { id: '7' }, cookies: await signedIn('jwt') }))

    expect(backend.requests.map(request => request.url)).toEqual(['/api/me/services/7/hosting-info'])
  })

  it('refuses a malformed id with the coded 400 before calling the backend', async () => {
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
