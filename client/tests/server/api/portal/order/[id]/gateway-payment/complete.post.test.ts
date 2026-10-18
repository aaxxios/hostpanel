/**
 * Tests for `POST /api/portal/order/:id/gateway-payment/complete`: the checkout return page
 * asking the backend, anonymously, how a hosted-gateway payment ended.
 *
 * @module tests/server/api/portal/order/[id]/gateway-payment/complete.post.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend } from '~/tests/helpers/bffHarness'
import type { RouteModule } from '~/tests/helpers/bffHarness'
import type { StubServer } from '~/tests/helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/order/[id]/gateway-payment/complete.post')
})

afterEach(async () => {
  await backend.close()
})

describe('POST /api/portal/order/:id/gateway-payment/complete', () => {
  it.each(['paid', 'pending', 'declined'])('relays the bank outcome %s', async (state) => {
    backend.respondWith(() => ({ body: { state } }))

    const result = await route.default(makeEvent({ params: { id: '42' } }))

    expect(result).toEqual({ state })
    expect(backend.requests[0]?.method).toBe('POST')
    expect(backend.requests[0]?.url).toBe('/api/orders/42/gateway-payment/complete')
  })

  it('refuses a request with no order id before reaching the backend', async () => {
    const error = await refusal(() => route.default(makeEvent()))

    expect(error).toMatchObject({ statusCode: 400, data: { code: 'BAD_ROUTE_PARAM' } })
    expect(backend.requests).toHaveLength(0)
  })
})
