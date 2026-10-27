/**
 * Tests for `POST /api/portal/client/domains/:id/renew-order`: it maps the portal form to the
 * backend renew contract for the domain in the path and passes the answer or refusal through.
 *
 * @module tests/server/api/portal/client/domains/[id]/renew-order.post.test
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
  route = await import('~/server/api/portal/client/domains/[id]/renew-order.post')
})

afterEach(async () => {
  await backend.close()
})

describe('POST /api/portal/client/domains/:id/renew-order', () => {
  it('posts years and payment method to the renew endpoint of that domain with the bearer and language', async () => {
    backend.respondWith(() => ({ body: { orderId: 2, invoiceId: 3 } }))

    const result = await route.default(makeEvent({
      params: { id: '7' },
      body: { years: 2, paymentmethod: 'card', ignored: 1 },
      cookies: await signedIn('jwt'),
      headers: { 'x-language': 'ru' },
    }))

    expect(result).toEqual({ orderId: 2, invoiceId: 3 })
    expect(backend.requests[0]?.method).toBe('POST')
    expect(backend.requests[0]?.url).toBe('/api/me/domains/7/renew')
    expect(backend.requests[0]?.headers['authorization']).toBe('Bearer jwt')
    expect(backend.requests[0]?.headers['x-language']).toBe('ru')
    expect(JSON.parse(backend.requests[0]?.body ?? '{}')).toEqual({ years: 2, paymentMethod: 'card' })
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 409, body: { type: 'DOMAIN_NOT_RENEWABLE', detail: 'Too early.' } }))

    const error = await refusal(async () => route.default(makeEvent({ params: { id: '7' }, body: {}, cookies: await signedIn('jwt') })))

    expect(error).toMatchObject({ statusCode: 409, statusMessage: 'Too early.', data: { code: 'DOMAIN_NOT_RENEWABLE' } })
  })
})
