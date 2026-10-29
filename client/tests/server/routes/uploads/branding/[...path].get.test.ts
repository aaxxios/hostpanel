/**
 * Tests for `GET /uploads/branding/**`: it serves only png and ico branding files from the C# API,
 * refuses path traversal, and sends long-lived, no-sniff headers.
 *
 * @module tests/server/routes/uploads/branding/[...path].get.test
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
  route = await import('~/server/routes/uploads/branding/[...path].get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /uploads/branding/**', () => {
  it('returns the file bytes with the png content type and immutable cache headers', async () => {
    backend.respondWith(() => ({ body: 'png-bytes' }))
    const event = makeEvent({ params: { path: 'abc/v1/icon-192.png' } })

    const body = await route.default(event) as Buffer

    expect(body.toString()).toBe('png-bytes')
    expect(backend.requests[0]?.url).toBe('/uploads/branding/abc/v1/icon-192.png')
    expect(event.responseHeaders).toEqual({
      'Content-Type': 'image/png',
      'X-Content-Type-Options': 'nosniff',
      'Cache-Control': 'public, max-age=31536000, immutable',
    })
  })

  it('serves an ico file with the icon content type, whatever the extension case', async () => {
    const event = makeEvent({ params: { path: 'abc/favicon.ICO' } })

    await route.default(event)

    expect(event.responseHeaders['Content-Type']).toBe('image/x-icon')
  })

  it('refuses an empty path, a backslash, dot segments and their encoded forms with 400, without calling the backend', async () => {
    const paths = [
      '', 'a\\b.png', '../secret.png', 'a/./b.png', 'a//b.png',
      '%2e%2e/secret.png', '..%2fsecret.png', 'a%5cb.png', '%252e%252e/secret.png',
    ]
    for (const path of paths) {
      const error = await refusal(() => route.default(makeEvent({ params: { path } })))

      expect(error).toMatchObject({ statusCode: 400 })
    }
    expect(backend.requests).toHaveLength(0)
  })

  it('refuses any other file type with 404, without calling the backend', async () => {
    for (const path of ['abc/readme.txt', 'abc/logo.svg', 'abc/noextension']) {
      const error = await refusal(() => route.default(makeEvent({ params: { path } })))

      expect(error).toMatchObject({ statusCode: 404 })
    }
    expect(backend.requests).toHaveLength(0)
  })

  it('answers 404 when the backend does not have the file', async () => {
    backend.respondWith(() => ({ status: 404, body: { type: 'NOT_FOUND', detail: 'Missing.' } }))

    const error = await refusal(() => route.default(makeEvent({ params: { path: 'abc/gone.png' } })))

    expect(error).toMatchObject({ statusCode: 404 })
  })
})
