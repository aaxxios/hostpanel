/**
 * Tests for `GET /api/portal/public/tld-pricing`: it forwards every query parameter to the TLD
 * pricing endpoint and passes the answer or refusal through.
 *
 * @module tests/server/api/portal/public/tld-pricing.get.test
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
  backend = await startBackend()
  route = await import('~/server/api/portal/public/tld-pricing.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/public/tld-pricing', () => {
  it('requests the bare endpoint without a query, forwarding the reader language', async () => {
    backend.respondWith(() => ({ body: { tlds: [] } }))

    const result = await route.default(makeEvent({ headers: { 'x-language': 'ru' } }))

    expect(result).toEqual({ tlds: [] })
    expect(backend.requests[0]?.url).toBe('/api/domains/tld-pricing')
    expect(backend.requests[0]?.headers['x-language']).toBe('ru')
  })

  it('forwards every query parameter', async () => {
    await route.default(makeEvent({ query: { currency: 'USD', search: 'com' } }))

    const query = new URL(backend.requests[0]?.url ?? '', 'http://stub').searchParams
    expect(query.get('currency')).toBe('USD')
    expect(query.get('search')).toBe('com')
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 503, body: { type: 'UNAVAILABLE', detail: 'Try later.' } }))

    const error = await refusal(() => route.default(makeEvent()))

    expect(error).toMatchObject({ statusCode: 503, statusMessage: 'Try later.', data: { code: 'UNAVAILABLE' } })
  })
})
