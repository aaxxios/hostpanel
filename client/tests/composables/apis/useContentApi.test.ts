/**
 * Tests for `composables/apis/useContentApi.ts`.
 *
 * Each endpoint function is called for real through `apiFetch` against a stub server, and the
 * test asserts on the path, verb, query and body that arrived and on how the reply was typed.
 *
 * @module tests/composables/apis/useContentApi.test
 */

import { afterEach, describe, expect, it } from 'vitest'
import { expectLastRequest, startTransport } from '../../helpers/stubTransport'
import type { ExpectedRequest } from '../../helpers/stubTransport'
import type { StubServer } from '../../helpers/stubServer'
import { useContentApi } from '~/composables/apis/useContentApi'

/** The stub started by the current test. */
let stub: StubServer | undefined

afterEach(async () => {
  await stub?.close()
  stub = undefined
})

/** The api surface under test. */
const api = useContentApi()

/** Every endpoint function with the request it must send. */
const rows: ExpectedRequest[] = [
  { name: 'fetchAnnouncements reads the list', call: () => api.fetchAnnouncements(), method: 'GET', url: '/api/portal/client/announcements' },
  { name: 'fetchRecentAnnouncements reads the dashboard summaries', call: () => api.fetchRecentAnnouncements(), method: 'GET', url: '/api/portal/public/announcements' },
  { name: 'fetchKbCategories reads the list', call: () => api.fetchKbCategories(), method: 'GET', url: '/api/portal/knowledgebase/categories' },
  { name: 'fetchKbArticles sends the filters as a query', call: () => api.fetchKbArticles({ categoryid: '3', limitnum: 100 }), method: 'GET', url: '/api/portal/knowledgebase/articles?categoryid=3&limitnum=100' },
  { name: 'fetchKbArticles sends a search term', call: () => api.fetchKbArticles({ search: 'vps', limitnum: 50 }), method: 'GET', url: '/api/portal/knowledgebase/articles?search=vps&limitnum=50' },
  { name: 'fetchKbArticle reads one article by id', call: () => api.fetchKbArticle('12'), method: 'GET', url: '/api/portal/knowledgebase/12' },
]

describe('useContentApi', () => {
  it.each(rows)('$name', async (row) => {
    stub = await startTransport()

    await row.call()

    expectLastRequest(stub, row)
  })

  it('returns the article envelope as the endpoint promises', async () => {
    stub = await startTransport(() => ({ body: { items: [{ id: 1, title: 'a' }] } }))

    const list = await api.fetchKbArticles({ limitnum: 10 })

    expect(list.items[0]?.title).toBe('a')
  })
})
