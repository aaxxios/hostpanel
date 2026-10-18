/**
 * Tests for `POST /api/portal/client/services/:id/change-password`: it forwards to the signed-in `me/services/{id}/change-password` endpoint and renames the field the page posts to the one the backend command reads.
 *
 * @module tests/server/api/portal/client/services/[id]/change-password.post.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, startBackend } from '../../../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../../../helpers/bffHarness'
import type { StubServer } from '../../../../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/client/services/[id]/change-password.post')
})

afterEach(async () => {
  await backend.close()
})

describe('service change-password route', () => {
  it('sends the page field under the name the command reads', async () => {
    await route.default(makeEvent({ params: { id: '5' }, body: { password: 'Secret-123' }, cookies: { auth_token: 'jwt' } }))

    expect(backend.requests[0]?.url).toBe('/api/me/services/5/change-password')
    expect(JSON.parse(backend.requests[0]?.body ?? '{}')).toEqual({ newPassword: 'Secret-123' })
  })
})
