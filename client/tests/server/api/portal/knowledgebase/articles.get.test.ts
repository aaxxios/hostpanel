/**
 * Tests for `GET /api/portal/knowledgebase/articles`: it forwards the filters and paging to the
 * backend unchanged and passes the backend page through as sent.
 *
 * @module tests/server/api/portal/knowledgebase/articles.get.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend } from '../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../helpers/bffHarness'
import type { StubServer } from '../../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

/** The page the backend answers. */
const page = {
  items: [
    { id: 1, title: 'Set up mail', excerpt: 'Open the panel', category: 'Email' },
    { id: 3, title: 'Mail forwarding', excerpt: 'Forward rules', category: 'email' },
  ],
  pageNumber: 2,
  pageSize: 2,
  totalCount: 4,
  totalPages: 2,
}

beforeEach(async () => {
  backend = await startBackend(() => ({ body: page }))
  route = await import('~/server/api/portal/knowledgebase/articles.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/knowledgebase/articles', () => {
  it('forwards the query and the reader language to the backend', async () => {
    await route.default(makeEvent({ query: { category: 'Email', search: 'mail', page: '2', pageSize: '2' }, headers: { 'x-language': 'hy' } }))

    expect(backend.requests[0]?.url).toBe('/api/knowledgebase/articles?category=Email&search=mail&page=2&pageSize=2')
    expect(backend.requests[0]?.headers['x-language']).toBe('hy')
  })

  it('passes the backend page through as sent', async () => {
    const result = await route.default(makeEvent())

    expect(result).toEqual(page)
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 503, body: { type: 'UNAVAILABLE', detail: 'Try later.' } }))

    const error = await refusal(() => route.default(makeEvent()))

    expect(error).toMatchObject({ statusCode: 503, statusMessage: 'Try later.', data: { code: 'UNAVAILABLE' } })
  })
})
