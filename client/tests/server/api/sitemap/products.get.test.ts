/**
 * Tests for `GET /api/sitemap/products`: one entry per active, non-empty product group except
 * hosting, built from the catalogue at request time, and no entries when the catalogue cannot
 * be read.
 *
 * @module tests/server/api/sitemap/products.get.test
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { makeEvent, startBackend } from '../../../helpers/bffHarness'
import type { RouteModule } from '../../../helpers/bffHarness'
import type { StubServer } from '../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend(() => ({
    body: [
      { id: 1, name: 'Web Hosting', productCount: 4 },
      { id: 3, name: 'Alpha Suite', productCount: 2 },
      { id: 4, name: 'Beta Kit', productCount: 0 }
    ]
  }))
  route = await import('~/server/api/sitemap/products.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/sitemap/products', () => {
  it('lists the active, non-empty families except hosting, which has its own page', async () => {
    const result = await route.default(makeEvent())

    expect(backend.requests[0]?.url).toBe('/api/public/product-groups')
    expect(result).toEqual([{ loc: '/products/alpha-suite', changefreq: 'weekly', priority: 0.8 }])
  })

  it('lists nothing when the catalogue cannot be read', async () => {
    backend.respondWith(() => ({ status: 500, body: { type: 'INTERNAL_ERROR', detail: 'Failed.' } }))

    const logged = vi.spyOn(console, 'error').mockImplementation(() => {})

    expect(await route.default(makeEvent())).toEqual([])
    expect(logged).toHaveBeenCalledOnce()
    logged.mockRestore()
  })
})
