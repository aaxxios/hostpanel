/**
 * Tests for `GET /api/portal/knowledgebase/:id`: it fetches one article and returns it with its
 * excerpt and full body.
 *
 * @module tests/server/api/portal/knowledgebase/[id].get.test
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
  route = await import('~/server/api/portal/knowledgebase/[id].get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/knowledgebase/:id', () => {
  it('returns the article with a tag-free excerpt and the original body', async () => {
    backend.respondWith(() => ({ body: { id: 9, title: 'Guide', content: '<h1>Hi</h1><p>There</p>', category: 'Email' } }))

    const result = await route.default(makeEvent({ params: { id: '9' }, headers: { 'x-language': 'hy' } }))

    expect(backend.requests[0]?.url).toBe('/api/knowledgebase/9')
    expect(backend.requests[0]?.headers['x-language']).toBe('hy')
    expect(result).toEqual({ id: 9, title: 'Guide', excerpt: 'Hi There', body: '<h1>Hi</h1><p>There</p>' })
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 404, body: { type: 'ARTICLE_NOT_FOUND', detail: 'No such article.' } }))

    const error = await refusal(() => route.default(makeEvent({ params: { id: '9' } })))

    expect(error).toMatchObject({ statusCode: 404, statusMessage: 'No such article.', data: { code: 'ARTICLE_NOT_FOUND' } })
  })
})
