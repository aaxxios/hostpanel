/**
 * Tests for `GET /api/portal/public/product-groups`: it forwards the catalogue's group list
 * untouched and passes a backend refusal through.
 *
 * @module tests/server/api/portal/public/product-groups.get.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend } from '../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../helpers/bffHarness'
import type { StubServer } from '../../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend(() => ({ body: [{ id: 3, name: 'Alpha Suite', isActive: true, productCount: 2 }] }))
  route = await import('~/server/api/portal/public/product-groups.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/public/product-groups', () => {
  it('lists the groups as the backend sent them, forwarding the reader language', async () => {
    const result = await route.default(makeEvent({ headers: { 'x-language': 'hy' } }))

    expect(backend.requests[0]?.url).toBe('/api/products/groups')
    expect(backend.requests[0]?.headers['x-language']).toBe('hy')
    expect(result).toEqual([{ id: 3, name: 'Alpha Suite', isActive: true, productCount: 2 }])
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 503, body: { type: 'UNAVAILABLE', detail: 'Try later.' } }))

    const error = await refusal(() => route.default(makeEvent()))

    expect(error).toMatchObject({ statusCode: 503, statusMessage: 'Try later.', data: { code: 'UNAVAILABLE' } })
  })
})
