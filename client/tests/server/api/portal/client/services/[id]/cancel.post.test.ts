/**
 * Tests for `POST /api/portal/client/services/:id/cancel`: it forwards the body to the signed-in `me/services/{id}/cancel` endpoint and writes no sentence of its own on a refusal.
 *
 * @module tests/server/api/portal/client/services/[id]/cancel.post.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend, signedIn } from '../../../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../../../helpers/bffHarness'
import type { StubServer } from '../../../../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/client/services/[id]/cancel.post')
})

afterEach(async () => {
  await backend.close()
})

describe('service cancel route', () => {
  it('posts the body to the service cancel endpoint', async () => {
    await route.default(makeEvent({ params: { id: '5' }, body: { reason: 'moving' }, cookies: await signedIn('jwt') }))

    expect(backend.requests[0]?.url).toBe('/api/me/services/5/cancel')
    expect(JSON.parse(backend.requests[0]?.body ?? '{}')).toEqual({ reason: 'moving' })
  })

  it('writes no sentence of its own when the backend refuses a cancellation', async () => {
    backend.respondWith(() => ({ status: 409, body: { type: 'SERVICE_NOT_ACTIVE', detail: 'Not active.' } }))

    const error = await refusal(async () => route.default(makeEvent({ params: { id: '5' }, body: {}, cookies: await signedIn('jwt') })))

    expect(error).toMatchObject({ statusCode: 409, statusMessage: 'Not active.', data: { code: 'SERVICE_NOT_ACTIVE' } })
  })
})
