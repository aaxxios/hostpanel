/**
 * Tests for `GET /api/portal/public/announcements`: it reads the ten newest published announcements
 * and reduces each to id, title and calendar date.
 *
 * @module tests/server/api/portal/public/announcements.get.test
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
  route = await import('~/server/api/portal/public/announcements.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/public/announcements', () => {
  it('maps announcements to id, title and the date part of the publish time', async () => {
    backend.respondWith(() => ({
      body: { items: [{ id: 1, title: 'News', publishedAt: '2026-03-04T12:30:00Z', content: 'x' }, { id: 2, title: 'Undated' }] },
    }))

    const result = await route.default(makeEvent({ headers: { 'x-language': 'hy' } }))

    expect(backend.requests[0]?.url).toBe('/api/announcements/published?page=1&pageSize=10')
    expect(backend.requests[0]?.headers['x-language']).toBe('hy')
    expect(result).toEqual([{ id: 1, title: 'News', date: '2026-03-04' }, { id: 2, title: 'Undated', date: '' }])
  })

  it('answers an empty list when the backend sends no items', async () => {
    backend.respondWith(() => ({ body: {} }))

    expect(await route.default(makeEvent())).toEqual([])
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 503, body: { type: 'UNAVAILABLE', detail: 'Try later.' } }))

    const error = await refusal(() => route.default(makeEvent()))

    expect(error).toMatchObject({ statusCode: 503, statusMessage: 'Try later.', data: { code: 'UNAVAILABLE' } })
  })
})
