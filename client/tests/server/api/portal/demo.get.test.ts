/**
 * Tests for `GET /api/portal/demo`: it proxies the anonymous public demo endpoint and answers
 * "not a demo" instead of an error when the backend cannot supply it.
 *
 * @module tests/server/api/portal/demo.get.test
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
  route = await import('~/server/api/portal/demo.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/demo', () => {
  it('passes the status of the public endpoint through', async () => {
    const status = {
      enabled: true,
      nextResetAt: '2030-01-01T00:00:00Z',
      accounts: [{ role: 'Client', email: 'client@demo.test', password: 'x' }]
    }
    backend.respondWith(() => ({ body: status }))

    const result = await route.default(makeEvent())

    expect(result).toEqual(status)
    expect(backend.requests[0]?.url).toBe('/api/public/demo')
  })

  it('answers not-a-demo when the backend refuses', async () => {
    backend.respondWith(() => ({ status: 404, body: { type: 'NOT_FOUND', detail: 'No such route.' } }))

    expect(await route.default(makeEvent())).toEqual({ enabled: false, nextResetAt: null, accounts: [] })
  })
})
