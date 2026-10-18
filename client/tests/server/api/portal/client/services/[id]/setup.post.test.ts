/**
 * Tests for `POST /api/portal/client/services/:id/setup`: it forwards the body to the signed-in `me/services/{id}/setup` endpoint and refuses a missing id segment before any call.
 *
 * @module tests/server/api/portal/client/services/[id]/setup.post.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend } from '../../../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../../../helpers/bffHarness'
import type { StubServer } from '../../../../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/client/services/[id]/setup.post')
})

afterEach(async () => {
  await backend.close()
})

describe('service setup route', () => {
  it('posts the body to the service setup endpoint', async () => {
    backend.respondWith(() => ({ body: { ok: true } }))

    const result = await route.default(makeEvent({ params: { id: '5' }, body: { domain: 'a.test' }, cookies: { auth_token: 'jwt' } }))

    expect(result).toEqual({ ok: true })
    expect(backend.requests[0]?.method).toBe('POST')
    expect(backend.requests[0]?.url).toBe('/api/me/services/5/setup')
    expect(backend.requests[0]?.headers['authorization']).toBe('Bearer jwt')
    expect(JSON.parse(backend.requests[0]?.body ?? '{}')).toEqual({ domain: 'a.test' })
  })

  it('refuses the call when the id segment is missing', async () => {
    const error = await refusal(() => route.default(makeEvent({ body: {} })))

    expect(error).toMatchObject({ statusCode: 400, data: { code: 'BAD_ROUTE_PARAM' } })
    expect(backend.requests).toHaveLength(0)
  })
})
