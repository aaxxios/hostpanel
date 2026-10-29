/**
 * Tests for `GET /api/portal/public/announcements/:id`: it reads one published announcement from
 * the backend's public detail route, passes it through, and refuses a malformed id.
 *
 * @module tests/server/api/portal/public/announcements/[id].get.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend } from '../../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../../helpers/bffHarness'
import type { StubServer } from '../../../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/public/announcements/[id].get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/public/announcements/:id', () => {
  it('reads the announcement by id and passes it through with the reader language', async () => {
    const announcement = { id: 11, title: 'Maintenance', content: '<p>x</p>', publishedAt: '2026-03-15T12:00:00Z' }
    backend.respondWith(() => ({ body: announcement }))

    const result = await route.default(makeEvent({ params: { id: '11' }, headers: { 'x-language': 'ru' } }))

    expect(backend.requests[0]?.url).toBe('/api/public/announcements/11')
    expect(backend.requests[0]?.headers['x-language']).toBe('ru')
    expect(result).toEqual(announcement)
  })

  it('refuses a malformed id before calling the backend', async () => {
    const error = await refusal(() => route.default(makeEvent({ params: { id: '../x' } })))

    expect(error).toMatchObject({ statusCode: 400, data: { code: 'BAD_ROUTE_PARAM' } })
    expect(backend.requests).toHaveLength(0)
  })

  it('passes the backend refusal for an unknown announcement through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 404, body: { type: 'ANNOUNCEMENT_NOT_FOUND', detail: 'No such announcement.' } }))

    const error = await refusal(() => route.default(makeEvent({ params: { id: '99' } })))

    expect(error).toMatchObject({ statusCode: 404, statusMessage: 'No such announcement.', data: { code: 'ANNOUNCEMENT_NOT_FOUND' } })
  })
})
