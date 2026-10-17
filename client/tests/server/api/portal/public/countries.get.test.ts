/**
 * Tests for `GET /api/portal/public/countries`: it returns the reference list and answers an empty
 * list instead of an error when the backend cannot supply it.
 *
 * @module tests/server/api/portal/public/countries.get.test
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
  route = await import('~/server/api/portal/public/countries.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/public/countries', () => {
  it('returns the reference countries, forwarding the reader language', async () => {
    backend.respondWith(() => ({ body: [{ code: 'AM', name: 'Armenia' }] }))

    const result = await route.default(makeEvent({ headers: { 'x-language': 'hy' } }))

    expect(result).toEqual([{ code: 'AM', name: 'Armenia' }])
    expect(backend.requests[0]?.url).toBe('/api/reference/countries')
    expect(backend.requests[0]?.headers['x-language']).toBe('hy')
  })

  it('answers an empty list when the backend refuses', async () => {
    backend.respondWith(() => ({ status: 500, body: { type: 'INTERNAL_ERROR', detail: 'Failed.' } }))

    expect(await route.default(makeEvent())).toEqual([])
  })
})
