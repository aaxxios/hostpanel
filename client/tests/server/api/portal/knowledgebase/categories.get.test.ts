/**
 * Tests for `GET /api/portal/knowledgebase/categories`: it derives the category list, with article
 * counts, from the published articles.
 *
 * @module tests/server/api/portal/knowledgebase/categories.get.test
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
  route = await import('~/server/api/portal/knowledgebase/categories.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/knowledgebase/categories', () => {
  it('counts articles per category, sorted by name, skipping articles without one', async () => {
    backend.respondWith(() => ({
      body: [
        { id: 1, title: 'a', content: '', category: 'Email', isPublished: true },
        { id: 2, title: 'b', content: '', category: 'Domains', isPublished: true },
        { id: 3, title: 'c', content: '', category: 'Email', isPublished: true },
        { id: 4, title: 'd', content: '', category: '  ', isPublished: true },
      ],
    }))

    const result = await route.default(makeEvent({ headers: { 'x-language': 'ru' } }))

    expect(backend.requests[0]?.url).toBe('/api/knowledgebase')
    expect(backend.requests[0]?.headers['x-language']).toBe('ru')
    expect(result).toEqual([
      { id: 'Domains', name: 'Domains', articlecount: 1 },
      { id: 'Email', name: 'Email', articlecount: 2 },
    ])
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 503, body: { type: 'UNAVAILABLE', detail: 'Try later.' } }))

    const error = await refusal(() => route.default(makeEvent()))

    expect(error).toMatchObject({ statusCode: 503, statusMessage: 'Try later.', data: { code: 'UNAVAILABLE' } })
  })
})
