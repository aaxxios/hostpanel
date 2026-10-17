/**
 * Tests for `GET /api/portal/client/emails`: it reads the first 100 sent e-mails and reduces each
 * to id, date and subject.
 *
 * @module tests/server/api/portal/client/emails.get.test
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
  route = await import('~/server/api/portal/client/emails.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/client/emails', () => {
  it('maps log entries to string id, date and subject only', async () => {
    backend.respondWith(() => ({
      body: { items: [{ id: 5, to: 'a@b.test', subject: 'Invoice', sentAt: '2026-02-03T00:00:00Z', success: true, error: null }] },
    }))

    const result = await route.default(makeEvent({ cookies: { auth_token: 'jwt' }, headers: { 'x-language': 'hy' } }))

    expect(backend.requests[0]?.url).toBe('/api/me/emails?page=1&pageSize=100')
    expect(backend.requests[0]?.headers['authorization']).toBe('Bearer jwt')
    expect(backend.requests[0]?.headers['x-language']).toBe('hy')
    expect(result).toEqual([{ id: '5', date: '2026-02-03T00:00:00Z', subject: 'Invoice' }])
  })

  it('answers an empty list when the backend sends no items', async () => {
    backend.respondWith(() => ({ body: {} }))

    expect(await route.default(makeEvent())).toEqual([])
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 403, body: { type: 'FORBIDDEN', detail: 'No access.' } }))

    const error = await refusal(() => route.default(makeEvent()))

    expect(error).toMatchObject({ statusCode: 403, statusMessage: 'No access.', data: { code: 'FORBIDDEN' } })
  })
})
