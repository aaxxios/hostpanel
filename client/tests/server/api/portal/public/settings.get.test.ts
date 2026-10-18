/**
 * Tests for `GET /api/portal/public/settings`: it exposes only the allow-listed, non-empty public
 * settings and answers an empty object instead of an error when the backend cannot supply them.
 *
 * @module tests/server/api/portal/public/settings.get.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, startBackend } from '../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../helpers/bffHarness'
import type { StubServer } from '../../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/public/settings.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/public/settings', () => {
  it('keeps allow-listed keys with a value and drops unknown keys and empty values', async () => {
    backend.respondWith(() => ({
      body: [
        { key: 'portal.site.name', value: 'Hostpanel' },
        { key: 'portal.site.tagline', value: '' },
        { key: 'smtp.password', value: 'secret' },
        { key: 'portal.brand.primary', value: '#123456' },
      ],
    }))

    const result = await route.default(makeEvent({ headers: { 'x-language': 'ru' } }))

    expect(result).toEqual({ 'portal.site.name': 'Hostpanel', 'portal.brand.primary': '#123456' })
    expect(backend.requests[0]?.url).toBe('/api/settings/public')
    expect(backend.requests[0]?.headers['x-language']).toBe('ru')
  })

  it('answers an empty object when the backend sends nothing', async () => {
    backend.respondWith(() => ({ body: null }))

    expect(await route.default(makeEvent())).toEqual({})
  })

  it('answers an empty object when the backend refuses', async () => {
    backend.respondWith(() => ({ status: 500, body: { type: 'INTERNAL_ERROR', detail: 'Failed.' } }))

    expect(await route.default(makeEvent())).toEqual({})
  })
})
