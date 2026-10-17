/**
 * Tests for `GET /site.webmanifest`: it builds the web-app manifest from the public settings,
 * using the generated icon set next to the configured favicon or the default icons.
 *
 * @module tests/server/routes/site.webmanifest.get.test
 */

import { ofetch } from 'ofetch'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { makeEvent, startBackend } from '../../helpers/bffHarness'
import type { RouteModule } from '../../helpers/bffHarness'
import type { StubServer } from '../../helpers/stubServer'

/** The stub playing the portal's own `/api/portal/public/settings` route. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

/** The shape of the manifest the route returns. */
interface Manifest {
  /** The app name. */
  name: string
  /** The declared icons. */
  icons: { src: string, sizes: string, type: string, purpose: string }[]
  /** The display mode. */
  display: string
}

beforeEach(async () => {
  backend = await startBackend()
  // The route asks the portal's own settings route by a relative address; the stub plays that route.
  vi.stubGlobal('$fetch', (url: string) => ofetch(url, { baseURL: backend.baseUrl }))
  route = await import('~/server/routes/site.webmanifest.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /site.webmanifest', () => {
  it('points the icons at the generated set that sits next to the configured favicon', async () => {
    backend.respondWith(() => ({ body: { 'portal.favicon': '/uploads/branding/abc/v1/favicon.ico' } }))
    const event = makeEvent()

    const manifest = await route.default(event) as Manifest

    expect(backend.requests[0]?.url).toBe('/api/portal/public/settings')
    expect(manifest.icons.map(icon => icon.src)).toEqual([
      '/uploads/branding/abc/v1/icon-192.png',
      '/uploads/branding/abc/v1/icon-512.png',
      '/uploads/branding/abc/v1/icon-maskable-192.png',
      '/uploads/branding/abc/v1/icon-maskable-512.png',
    ])
    expect(manifest.icons.map(icon => icon.purpose)).toEqual(['any', 'any', 'maskable', 'maskable'])
    expect(manifest).toMatchObject({ name: 'Hostpanel', display: 'standalone' })
    expect(event.responseHeaders).toMatchObject({ 'Content-Type': 'application/manifest+json', 'Cache-Control': 'public, max-age=60' })
  })

  it('falls back to the default icons when the favicon is not a generated set', async () => {
    backend.respondWith(() => ({ body: { 'portal.favicon': '/uploads/branding/favicon.ico' } }))

    const manifest = await route.default(makeEvent()) as Manifest

    expect(manifest.icons.map(icon => icon.src)).toEqual(['/icon-192.png', '/icon-512.png', '/icon-maskable-192.png', '/icon-maskable-512.png'])
  })

  it('falls back to the default icons when the settings cannot be read', async () => {
    backend.respondWith(() => ({ status: 500, body: { type: 'INTERNAL_ERROR', detail: 'Failed.' } }))

    const manifest = await route.default(makeEvent()) as Manifest

    expect(manifest.icons[0]?.src).toBe('/icon-192.png')
  })
})
