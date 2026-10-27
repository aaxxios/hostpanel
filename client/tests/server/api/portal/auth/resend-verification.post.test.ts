/**
 * Tests for `POST /api/portal/auth/resend-verification`: it calls the backend `POST /auth/resend-verification` with the session bearer and no body, forwards the reader language and passes a refusal through.
 *
 * @module tests/server/api/portal/auth/resend-verification.post.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend, signedIn } from '../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../helpers/bffHarness'
import type { StubServer } from '../../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/auth/resend-verification.post')
})

afterEach(async () => {
  await backend.close()
})

describe('POST /api/portal/auth/resend-verification', () => {
  it('calls the backend endpoint with the session bearer', async () => {
    await route.default(makeEvent({ cookies: await signedIn('jwt') }))

    expect(backend.requests[0]?.method).toBe('POST')
    expect(backend.requests[0]?.url).toBe('/api/auth/resend-verification')
    expect(backend.requests[0]?.headers['authorization']).toBe('Bearer jwt')
  })

  it('forwards the reader language to the backend', async () => {
    await route.default(makeEvent({ headers: { 'x-language': 'ru' }, cookies: await signedIn('jwt') }))

    expect(backend.requests[0]?.headers['x-language']).toBe('ru')
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 400, body: { type: 'USER_PROVISIONING_NOT_ALLOWED_ISSUE_EMAIL_CONFIRMATION', detail: 'Managed by SSO.' } }))

    const error = await refusal(async () => route.default(makeEvent({ cookies: await signedIn('jwt') })))

    expect(error).toMatchObject({ statusCode: 400, statusMessage: 'Managed by SSO.', data: { code: 'USER_PROVISIONING_NOT_ALLOWED_ISSUE_EMAIL_CONFIRMATION' } })
  })
})
