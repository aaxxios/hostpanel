/**
 * Tests for `GET /api/portal/knowledgebase/categories`: it passes the backend's category list,
 * with article counts, through as sent.
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
  it('reads the backend category list and passes it through as sent', async () => {
    const categories = [
      { name: 'Domains', articleCount: 1 },
      { name: 'Email', articleCount: 2 },
    ]
    backend.respondWith(() => ({ body: categories }))

    const result = await route.default(makeEvent({ headers: { 'x-language': 'ru' } }))

    expect(backend.requests[0]?.url).toBe('/api/knowledgebase/categories')
    expect(backend.requests[0]?.headers['x-language']).toBe('ru')
    expect(result).toEqual(categories)
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 503, body: { type: 'UNAVAILABLE', detail: 'Try later.' } }))

    const error = await refusal(() => route.default(makeEvent()))

    expect(error).toMatchObject({ statusCode: 503, statusMessage: 'Try later.', data: { code: 'UNAVAILABLE' } })
  })
})
