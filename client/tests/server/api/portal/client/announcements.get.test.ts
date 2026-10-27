/**
 * Tests for `GET /api/portal/client/announcements`: it pages the published announcements from the
 * backend and reshapes each into the portal's list item.
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
  it('requests page 1 of 50 by default and maps each announcement to id, title, date and body', async () => {
    backend.respondWith(() => ({
      body: { items: [{ id: 3, title: 'Maintenance', publishedAt: '2026-01-02T10:00:00Z', content: 'Tonight.' }, { id: 4, title: 'Draft' }] },
    }))

    const result = await route.default(makeEvent({ cookies: await signedIn('jwt'), headers: { 'x-language': 'ru' } }))

    expect(backend.requests[0]?.url).toBe('/api/announcements/published?page=1&pageSize=50')
    expect(backend.requests[0]?.headers['x-language']).toBe('ru')
    expect(result).toEqual({
      items: [
        { id: 3, title: 'Maintenance', date: '2026-01-02T10:00:00Z', body: 'Tonight.' },
        { id: 4, title: 'Draft', date: '', body: '' },
      ],
    })
  })

  it('forwards the requested page and page size', async () => {
    backend.respondWith(() => ({ body: { items: [] } }))

    await route.default(makeEvent({ query: { page: '2', pageSize: '10' } }))

    expect(backend.requests[0]?.url).toBe('/api/announcements/published?page=2&pageSize=10')
  })

  it('answers an empty list when the backend sends no items', async () => {
    backend.respondWith(() => ({ body: {} }))

    expect(await route.default(makeEvent())).toEqual({ items: [] })
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 503, body: { type: 'UNAVAILABLE', detail: 'Try later.' } }))

    const error = await refusal(() => route.default(makeEvent()))

    expect(error).toMatchObject({ statusCode: 503, statusMessage: 'Try later.', data: { code: 'UNAVAILABLE' } })
  })
})
