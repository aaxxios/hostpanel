/**
 * Tests for `GET /api/portal/client/services`: it lists the signed-in client's services and maps
 * each to the portal's service row, with defaults for what the list does not carry.
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
  it('maps the backend rows with the bearer and the reader language', async () => {
    backend.respondWith(() => ({
      body: [{ id: 4, productId: 12, productName: 'Hosting S', domain: 'a.test', price: '9.99', billingCycle: 'Monthly', nextRenewalAt: '2026-05-01', status: 'Suspended', username: 'u1', createdAt: '2026-01-05T09:00:00Z' }],
    }))

    const result = await route.default(makeEvent({ cookies: await signedIn('jwt'), headers: { 'x-language': 'ru' } })) as Record<string, unknown>[]

    expect(backend.requests[0]?.url).toBe('/api/me/services')
    expect(backend.requests[0]?.headers['authorization']).toBe('Bearer jwt')
    expect(backend.requests[0]?.headers['x-language']).toBe('ru')
    expect(result[0]).toMatchObject({
      id: 4,
      pid: 12,
      regdate: '2026-01-05T09:00:00Z',
      name: 'Hosting S',
      translated_name: 'Hosting S',
      domain: 'a.test',
      recurringamount: '9.99',
      billingcycle: 'Monthly',
      nextduedate: '2026-05-01',
      status: 'Suspended',
      username: 'u1',
    })
  })

  it('fills the defaults when the backend row omits optional fields', async () => {
    backend.respondWith(() => ({ body: [{ id: 1, productId: 2, productName: 'P' }] }))

    const result = await route.default(makeEvent({ cookies: await signedIn('jwt') })) as Record<string, unknown>[]

    expect(result[0]).toMatchObject({ domain: '', recurringamount: '0.00', billingcycle: '', nextduedate: '', status: 'Active', username: 'provisioned' })
  })

  it('keeps a null product name null for the page to word', async () => {
    backend.respondWith(() => ({ body: [{ id: 1, productId: 2, productName: null }] }))

    const result = await route.default(makeEvent({ cookies: await signedIn('jwt') })) as Record<string, unknown>[]

    expect(result[0]).toMatchObject({ name: null, translated_name: null })
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
