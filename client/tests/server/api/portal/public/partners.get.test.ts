/**
 * Tests for `GET /api/portal/public/partners`: it reads the public partners feed, forwards the
 * reader's language and passes the answer or refusal through.
 *
 * @module tests/server/api/portal/public/partners.get.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend } from '../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../helpers/bffHarness'
import type { StubServer } from '../../../../helpers/stubServer'
import { partnerFixture } from '../../../../helpers/storefrontFixtures'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/public/partners.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/public/partners', () => {
  it('requests the public feed, forwarding the reader language', async () => {
    backend.respondWith(() => ({ body: [partnerFixture(1)] }))

    const result = await route.default(makeEvent({ headers: { 'x-language': 'hy' } }))

    expect(result).toEqual([partnerFixture(1)])
    expect(backend.requests[0]?.url).toBe('/api/public/partners')
    expect(backend.requests[0]?.headers['x-language']).toBe('hy')
  })

  it('passes an empty feed through, which hides the section', async () => {
    backend.respondWith(() => ({ body: [] }))

    expect(await route.default(makeEvent())).toEqual([])
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 503, body: { type: 'UNAVAILABLE', detail: 'Try later.' } }))

    const error = await refusal(() => route.default(makeEvent()))

    expect(error).toMatchObject({ statusCode: 503, statusMessage: 'Try later.', data: { code: 'UNAVAILABLE' } })
  })
})
