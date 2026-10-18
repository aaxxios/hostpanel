/**
 * Tests for `POST /api/portal/client/domains/transfer-order`: it renames the portal form fields to
 * the backend transfer-order contract and passes the backend answer or refusal through.
 *
 * @module tests/server/api/portal/client/domains/transfer-order.post.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend } from '../../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../../helpers/bffHarness'
import type { StubServer } from '../../../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

/** The form the portal posts. */
const form = { domain: 'example.com', eppcode: 'abc', years: 1, paymentmethod: 'card', ignored: true }

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/client/domains/transfer-order.post')
})

afterEach(async () => {
  await backend.close()
})

describe('POST /api/portal/client/domains/transfer-order', () => {
  it('posts the renamed fields with the bearer and the reader language, and returns the order ids', async () => {
    backend.respondWith(() => ({ body: { orderId: 9, invoiceId: 11 } }))

    const result = await route.default(makeEvent({ body: form, cookies: { auth_token: 'jwt' }, headers: { 'x-language': 'hy' } }))

    expect(result).toEqual({ orderId: 9, invoiceId: 11 })
    expect(backend.requests[0]?.method).toBe('POST')
    expect(backend.requests[0]?.url).toBe('/api/me/domains/transfer-order')
    expect(backend.requests[0]?.headers['authorization']).toBe('Bearer jwt')
    expect(backend.requests[0]?.headers['x-language']).toBe('hy')
    expect(JSON.parse(backend.requests[0]?.body ?? '{}')).toEqual({
      domainName: 'example.com',
      eppCode: 'abc',
      years: 1,
      paymentMethod: 'card',
    })
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 422, body: { type: 'DOMAIN_NOT_TRANSFERABLE', detail: 'Locked.' } }))

    const error = await refusal(() => route.default(makeEvent({ body: form, cookies: { auth_token: 'jwt' } })))

    expect(error).toMatchObject({ statusCode: 422, statusMessage: 'Locked.', data: { code: 'DOMAIN_NOT_TRANSFERABLE' } })
  })
})
