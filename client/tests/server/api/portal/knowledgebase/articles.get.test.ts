/**
 * Tests for `GET /api/portal/knowledgebase/articles`: it filters the published articles by
 * category and search text, pages the result, and returns id, title and excerpt only.
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

/** The published articles the backend holds. */
const articles = [
  { id: 1, title: 'Set up mail', content: '<p>Open the <b>panel</b></p>', category: 'Email', isPublished: true },
  { id: 2, title: 'Renew a domain', content: 'Renewal steps', category: 'Domains', isPublished: true },
  { id: 3, title: 'Mail forwarding', content: 'Forward rules', category: 'email', isPublished: true },
]

beforeEach(async () => {
  backend = await startBackend(() => ({ body: articles }))
  route = await import('~/server/api/portal/knowledgebase/articles.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/knowledgebase/articles', () => {
  it('lists every article as id, title and a tag-free excerpt, forwarding the reader language', async () => {
    const result = await route.default(makeEvent({ headers: { 'x-language': 'hy' } }))

    expect(backend.requests[0]?.url).toBe('/api/knowledgebase')
    expect(backend.requests[0]?.headers['x-language']).toBe('hy')
    expect(result).toEqual({
      items: [
        { id: 1, title: 'Set up mail', excerpt: 'Open the panel' },
        { id: 2, title: 'Renew a domain', excerpt: 'Renewal steps' },
        { id: 3, title: 'Mail forwarding', excerpt: 'Forward rules' },
      ],
    })
  })

  it('filters by category without regard to case', async () => {
    const result = await route.default(makeEvent({ query: { categoryid: 'EMAIL' } })) as { items: { id: number }[] }

    expect(result.items.map(item => item.id)).toEqual([1, 3])
  })

  it('filters by search text in the title or the content', async () => {
    const byTitle = await route.default(makeEvent({ query: { search: ' RENEW ' } })) as { items: { id: number }[] }
    const byContent = await route.default(makeEvent({ query: { search: 'forward rules' } })) as { items: { id: number }[] }

    expect(byTitle.items.map(item => item.id)).toEqual([2])
    expect(byContent.items.map(item => item.id)).toEqual([3])
  })

  it('pages the result by start and count', async () => {
    const page = await route.default(makeEvent({ query: { limitstart: '1', limitnum: '1' } })) as { items: { id: number }[] }
    const tail = await route.default(makeEvent({ query: { limitstart: '2' } })) as { items: { id: number }[] }

    expect(page.items.map(item => item.id)).toEqual([2])
    expect(tail.items.map(item => item.id)).toEqual([3])
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 503, body: { type: 'UNAVAILABLE', detail: 'Try later.' } }))

    const error = await refusal(() => route.default(makeEvent()))

    expect(error).toMatchObject({ statusCode: 503, statusMessage: 'Try later.', data: { code: 'UNAVAILABLE' } })
  })
})
