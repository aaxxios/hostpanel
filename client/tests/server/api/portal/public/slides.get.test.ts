/**
 * Tests for `GET /api/portal/public/slides`: it asks the slides
 * endpoint without an audience (the backend decides it) and passes the answer or refusal through.
 *
 * @module tests/server/api/portal/public/slides.get.test
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
  route = await import('~/server/api/portal/public/slides.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/public/slides', () => {
  it('requests the slides without an audience, forwarding the reader language', async () => {
    backend.respondWith(() => ({ body: [{ id: 1 }] }))

    const result = await route.default(makeEvent({ headers: { 'x-language': 'hy' } }))

    expect(result).toEqual([{ id: 1 }])
    expect(backend.requests[0]?.url).toBe('/api/slides')
    expect(backend.requests[0]?.headers['x-language']).toBe('hy')
  })

  it('does not forward an audience a caller put in the query string', async () => {
    await route.default(makeEvent({ query: { audience: 'authenticated' } }))

    expect(backend.requests[0]?.url).toBe('/api/slides')
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 503, body: { type: 'UNAVAILABLE', detail: 'Try later.' } }))

    const error = await refusal(() => route.default(makeEvent()))

    expect(error).toMatchObject({ statusCode: 503, statusMessage: 'Try later.', data: { code: 'UNAVAILABLE' } })
  })
})
