/**
 * Tests for `POST /api/portal/public/domain-check`: it sends only the trimmed domain name to the
 * availability endpoint and passes the answer or refusal through.
 *
 * @module tests/server/api/portal/public/domain-check.post.test
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
  route = await import('~/server/api/portal/public/domain-check.post')
})

afterEach(async () => {
  await backend.close()
})

describe('POST /api/portal/public/domain-check', () => {
  it('posts the trimmed domain and returns the availability answer', async () => {
    backend.respondWith(() => ({ body: { available: true } }))

    const result = await route.default(makeEvent({ body: { domain: '  example.com ', other: 1 }, headers: { 'x-language': 'ru' } }))

    expect(result).toEqual({ available: true })
    expect(backend.requests[0]?.method).toBe('POST')
    expect(backend.requests[0]?.url).toBe('/api/domains/check')
    expect(backend.requests[0]?.headers['x-language']).toBe('ru')
    expect(JSON.parse(backend.requests[0]?.body ?? '{}')).toEqual({ domain: 'example.com' })
  })

  it('sends an empty domain when the body carries no string', async () => {
    await route.default(makeEvent({ body: { domain: 5 } }))

    expect(JSON.parse(backend.requests[0]?.body ?? '{}')).toEqual({ domain: '' })
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 400, body: { type: 'DOMAIN_INVALID', detail: 'Not a domain.' } }))

    const error = await refusal(() => route.default(makeEvent({ body: { domain: 'x' } })))

    expect(error).toMatchObject({ statusCode: 400, statusMessage: 'Not a domain.', data: { code: 'DOMAIN_INVALID' } })
  })
})
