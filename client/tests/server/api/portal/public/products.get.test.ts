/**
 * Tests for `GET /api/portal/public/products`: it validates the catalogue filters, forwards them
 * under the backend's own parameter names (the backend does the filtering), and passes each product
 * through as the backend sends it.
 *
 * @module tests/server/api/portal/public/products.get.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend, signedIn } from '../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../helpers/bffHarness'
import type { StubServer } from '../../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend(() => ({ body: [{ id: 1, name: 'S' }, { id: 2, name: 'M' }] }))
  route = await import('~/server/api/portal/public/products.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/public/products', () => {
  it('lists every product as sent, forwarding the reader language', async () => {
    const result = await route.default(makeEvent({ headers: { 'x-language': 'ru' } }))

    expect(backend.requests[0]?.url).toBe('/api/public/products')
    expect(backend.requests[0]?.headers['x-language']).toBe('ru')
    expect(result).toEqual([{ id: 1, name: 'S' }, { id: 2, name: 'M' }])
  })

  it('forwards the filters under the backend parameter names and does not filter itself', async () => {
    const result = await route.default(makeEvent({ query: { pid: '2', gid: '4', gids: '5,4', currency: 'usd' } }))

    const query = new URL(backend.requests[0]?.url ?? '', 'http://stub').searchParams
    expect(query.get('productId')).toBe('2')
    expect(query.get('groupId')).toBe('4')
    expect(query.getAll('groupIds')).toEqual(['4', '5'])
    expect(query.get('currency')).toBe('USD')
    expect(query.has('pid')).toBe(false)
    expect(query.has('gid')).toBe(false)
    expect(query.has('gids')).toBe(false)
    // The backend answered with two products; the route returns them untouched.
    expect(result).toEqual([{ id: 1, name: 'S' }, { id: 2, name: 'M' }])
  })

  it.each([
    ['a non-numeric product', { pid: 'abc' }],
    ['a product with a path in it', { pid: '1/../x' }],
    ['a negative product', { pid: '-1' }],
    ['a zero group', { gid: '0' }],
    ['an oversized group', { gid: '2147483648' }],
    ['a group with a fraction', { gid: '1.5' }],
    ['a group list with free text', { gids: '1,<script>' }],
    ['a group list with an empty entry', { gids: '1,,2' }],
    ['a group list with a zero', { gids: '1,0' }],
    ['a currency that is not three letters', { currency: 'USDX' }],
    ['a currency with digits', { currency: 'U5D' }],
    ['a currency with an injected parameter', { currency: 'USD&admin=1' }]
  ])('refuses %s with a coded 400 before calling the backend', async (_label, query) => {
    const error = await refusal(() => route.default(makeEvent({ query: query as Record<string, string> })))

    expect(error).toMatchObject({ statusCode: 400, data: { code: 'BAD_QUERY_PARAM' } })
    expect(backend.requests).toHaveLength(0)
  })

  it('sends the bearer for a signed-in reader', async () => {
    await route.default(makeEvent({ cookies: await signedIn('jwt') }))

    expect(backend.requests[0]?.headers['authorization']).toBe('Bearer jwt')
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 503, body: { type: 'UNAVAILABLE', detail: 'Try later.' } }))

    const error = await refusal(() => route.default(makeEvent()))

    expect(error).toMatchObject({ statusCode: 503, statusMessage: 'Try later.', data: { code: 'UNAVAILABLE' } })
  })
})
