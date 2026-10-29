/**
 * Tests for `GET /api/portal/client/services`: it lists the signed-in client's services exactly as
 * the backend sends them.
 *
 * @module tests/server/api/portal/client/services/index.get.test
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
  route = await import('~/server/api/portal/client/services/index.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/client/services', () => {
  it('passes the backend rows through unchanged with the bearer and the reader language', async () => {
    const row = { id: 4, productId: 12, productName: 'Hosting S', billingCycle: 'Monthly', status: 'Suspended', nextRenewalAt: '2026-05-01T00:00:00Z', domain: 'a.test', username: 'u1', price: 9.99, firstPaymentAmount: 9.99, paymentMethod: null, server: null, createdAt: '2026-01-05T09:00:00Z' }
    backend.respondWith(() => ({ body: [row] }))

    const result = await route.default(makeEvent({ cookies: await signedIn('jwt'), headers: { 'x-language': 'ru' } }))

    expect(backend.requests[0]?.url).toBe('/api/me/services')
    expect(backend.requests[0]?.headers['authorization']).toBe('Bearer jwt')
    expect(backend.requests[0]?.headers['x-language']).toBe('ru')
    expect(result).toEqual([row])
  })

  it('answers an empty list for a client without services', async () => {
    backend.respondWith(() => ({ body: [] }))

    expect(await route.default(makeEvent({ cookies: await signedIn('jwt') }))).toEqual([])
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 404, body: { type: 'CLIENT_PROFILE_NOT_FOUND', detail: 'No profile.' } }))

    const error = await refusal(async () => route.default(makeEvent({ cookies: await signedIn('jwt') })))

    expect(error).toMatchObject({ statusCode: 404, statusMessage: 'No profile.', data: { code: 'CLIENT_PROFILE_NOT_FOUND' } })
  })
})
