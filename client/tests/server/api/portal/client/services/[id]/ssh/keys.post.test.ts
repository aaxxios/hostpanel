/**
 * Tests for `POST /api/portal/client/services/:id/ssh/keys`: it forwards only `label` and
 * `publicKey`, answers 201 and leaves validation to the backend.
 *
 * @module tests/server/api/portal/client/services/[id]/ssh/keys.post.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend, signedIn } from '../../../../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../../../../helpers/bffHarness'
import type { StubServer } from '../../../../../../../helpers/stubServer'
import { HttpMethod } from '~/types/httpmethod'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/client/services/[id]/ssh/keys.post')
})

afterEach(async () => {
  await backend.close()
})

describe('POST /api/portal/client/services/:id/ssh/keys', () => {
  it('posts only the label and public key and answers 201', async () => {
    backend.respondWith(() => ({ body: { id: 'k1', label: 'laptop' } }))
    const event = makeEvent({
      method: HttpMethod.Post,
      params: { id: '7' },
      body: { label: 'laptop', publicKey: 'ssh-ed25519 AAAA', server: 'other', account: 'root' },
      cookies: await signedIn('jwt'),
    })

    const result = await route.default(event)

    expect(backend.requests[0]?.method).toBe('POST')
    expect(backend.requests[0]?.url).toBe('/api/me/services/7/ssh/keys')
    expect(JSON.parse(backend.requests[0]?.body ?? '{}')).toEqual({ label: 'laptop', publicKey: 'ssh-ed25519 AAAA' })
    expect(event.status).toBe(201)
    expect(result).toEqual({ id: 'k1', label: 'laptop' })
  })

  it('sends empty strings for missing or non-string fields', async () => {
    await route.default(makeEvent({ method: HttpMethod.Post, params: { id: '7' }, body: { label: 5 }, cookies: await signedIn('jwt') }))

    expect(JSON.parse(backend.requests[0]?.body ?? '{}')).toEqual({ label: '', publicKey: '' })
  })

  it('passes the backend validation refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 400, body: { type: 'VALIDATION_ERROR', detail: 'Invalid key.' } }))

    const error = await refusal(async () => route.default(makeEvent({ method: HttpMethod.Post, params: { id: '7' }, body: {}, cookies: await signedIn('jwt') })))

    expect(error).toMatchObject({ statusCode: 400, statusMessage: 'Invalid key.', data: { code: 'VALIDATION_ERROR' } })
  })
})
