/**
 * Tests for `GET /api/portal/public/product-features`: it forwards the group and product filters
 * and answers an empty list instead of an error when the backend cannot supply the features.
 *
 * @module tests/server/api/portal/public/product-features.get.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, startBackend } from '../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../helpers/bffHarness'
import type { StubServer } from '../../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/public/product-features.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/public/product-features', () => {
  it('requests every feature when no filter is given, forwarding the reader language', async () => {
    backend.respondWith(() => ({ body: [{ id: 1 }] }))

    const result = await route.default(makeEvent({ headers: { 'x-language': 'hy' } }))

    expect(result).toEqual([{ id: 1 }])
    expect(backend.requests[0]?.url).toBe('/api/product-features')
    expect(backend.requests[0]?.headers['x-language']).toBe('hy')
  })

  it('maps the gid and pid filters to the backend group and product ids', async () => {
    await route.default(makeEvent({ query: { gid: '3', pid: '8' } }))

    const query = new URL(backend.requests[0]?.url ?? '', 'http://stub').searchParams
    expect(query.get('groupId')).toBe('3')
    expect(query.get('productId')).toBe('8')
  })

  it('answers an empty list when the backend refuses', async () => {
    backend.respondWith(() => ({ status: 500, body: { type: 'INTERNAL_ERROR', detail: 'Failed.' } }))

    expect(await route.default(makeEvent())).toEqual([])
  })
})
