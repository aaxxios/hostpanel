/**
 * Tests for `GET /api/portal/public/products`: it forwards the catalogue filters, narrows to one
 * product when a pid is given, and adds the legacy `pid` field to each product.
 *
 * @module tests/server/api/portal/public/products.get.test
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
  backend = await startBackend(() => ({ body: [{ id: 1, name: 'S' }, { id: 2, name: 'M' }] }))
  route = await import('~/server/api/portal/public/products.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/public/products', () => {
  it('lists every product with a pid copy of its id, forwarding the reader language', async () => {
    const result = await route.default(makeEvent({ headers: { 'x-language': 'ru' } }))

    expect(backend.requests[0]?.url).toBe('/api/products')
    expect(backend.requests[0]?.headers['x-language']).toBe('ru')
    expect(result).toEqual([{ id: 1, name: 'S', pid: 1 }, { id: 2, name: 'M', pid: 2 }])
  })

  it('forwards the filters and keeps only the requested product', async () => {
    const result = await route.default(makeEvent({ query: { pid: '2', gid: '4', gids: '4,5', currency: 'USD' } }))

    const query = new URL(backend.requests[0]?.url ?? '', 'http://stub').searchParams
    expect(query.get('pid')).toBe('2')
    expect(query.get('gid')).toBe('4')
    expect(query.get('groupId')).toBe('4')
    expect(query.get('gids')).toBe('4,5')
    expect(query.get('currency')).toBe('USD')
    expect(result).toEqual([{ id: 2, name: 'M', pid: 2 }])
  })

  it('sends the bearer for a signed-in reader', async () => {
    await route.default(makeEvent({ cookies: { auth_token: 'jwt' } }))

    expect(backend.requests[0]?.headers['authorization']).toBe('Bearer jwt')
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 503, body: { type: 'UNAVAILABLE', detail: 'Try later.' } }))

    const error = await refusal(() => route.default(makeEvent()))

    expect(error).toMatchObject({ statusCode: 503, statusMessage: 'Try later.', data: { code: 'UNAVAILABLE' } })
  })
})
