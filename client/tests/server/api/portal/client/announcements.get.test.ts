/**
 * Tests for `GET /api/portal/client/announcements`: it asks the backend for a page of the published
 * announcements and passes that page through as sent.
 *
 * @module tests/server/api/portal/client/announcements.get.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend, signedIn } from '../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../helpers/bffHarness'
import type { StubServer } from '../../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/client/announcements.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/client/announcements', () => {
  it('requests page 1 of 50 by default and passes the backend page through as sent', async () => {
    /** The backend's page of published announcements. */
    const page = {
      items: [{ id: 3, title: 'Maintenance', content: 'Tonight.', publishedAt: '2026-01-02T10:00:00Z' }],
      pageNumber: 1,
      pageSize: 50,
      totalCount: 1,
      totalPages: 1,
    }
    backend.respondWith(() => ({ body: page }))

    const result = await route.default(makeEvent({ cookies: await signedIn('jwt'), headers: { 'x-language': 'ru' } }))

    expect(backend.requests[0]?.url).toBe('/api/announcements/published?page=1&pageSize=50')
    expect(backend.requests[0]?.headers['x-language']).toBe('ru')
    expect(result).toEqual(page)
  })

  it('forwards the requested page and page size', async () => {
    backend.respondWith(() => ({ body: { items: [] } }))

    await route.default(makeEvent({ query: { page: '2', pageSize: '10' } }))

    expect(backend.requests[0]?.url).toBe('/api/announcements/published?page=2&pageSize=10')
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 503, body: { type: 'UNAVAILABLE', detail: 'Try later.' } }))

    const error = await refusal(() => route.default(makeEvent()))

    expect(error).toMatchObject({ statusCode: 503, statusMessage: 'Try later.', data: { code: 'UNAVAILABLE' } })
  })
})
