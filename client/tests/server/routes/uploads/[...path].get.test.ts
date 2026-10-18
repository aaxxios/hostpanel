/**
 * Tests for `GET /uploads/**`: it proxies the request for an uploaded file to the same path on the
 * C# API.
 *
 * @module tests/server/routes/uploads/[...path].get.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, startBackend } from '../../../helpers/bffHarness'
import type { RouteModule } from '../../../helpers/bffHarness'
import type { StubServer } from '../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/routes/uploads/[...path].get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /uploads/**', () => {
  it('requests the same path under /uploads on the backend and returns its answer', async () => {
    backend.respondWith(() => ({ body: 'file-bytes' }))

    const result = await route.default(makeEvent({ params: { path: 'docs/a.pdf' } }))

    expect(result).toBe('file-bytes')
    expect(backend.requests[0]?.method).toBe('GET')
    expect(backend.requests[0]?.url).toBe('/uploads/docs/a.pdf')
  })
})
