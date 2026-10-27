/**
 * Tests for `POST /api/portal/client/invoices/:id/pay`: a signed-in client paying an invoice,
 * including the one-shot token refresh when the access token has expired.
 *
 * @module tests/server/api/portal/client/invoices/[id]/pay.post.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend, signedIn, storedSession } from '../../../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../../../helpers/bffHarness'
import type { StubServer } from '../../../../../../helpers/stubServer'

/** The stub playing the C# API and the SSO token endpoint. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/client/invoices/[id]/pay.post')
})

afterEach(async () => {
  await backend.close()
})

describe('POST /api/portal/client/invoices/:id/pay', () => {
  it('posts the pay method to the signed-in invoice endpoint with the bearer', async () => {
    backend.respondWith(() => ({ body: { paid: true } }))

    const result = await route.default(makeEvent({
      params: { id: '9' },
      body: { paymethodid: 4 },
      cookies: await signedIn('jwt'),
    }))

    expect(result).toEqual({ paid: true })
    expect(backend.requests[0]?.method).toBe('POST')
    expect(backend.requests[0]?.url).toBe('/api/me/invoices/9/pay')
    expect(backend.requests[0]?.headers['authorization']).toBe('Bearer jwt')
    expect(JSON.parse(backend.requests[0]?.body ?? '{}')).toEqual({ paymethodid: 4 })
  })

  it('refreshes an expired access token once and retries with the new one', async () => {
    backend.respondWith((request) => {
      if (request.url === '/connect/token') {
        return { body: { access_token: 'fresh', expires_in: 900 } }
      }
      return request.headers['authorization'] === 'Bearer fresh' ? { body: { paid: true } } : { status: 401, body: {} }
    })
    const cookies = await signedIn('old', { refreshToken: 'r' })
    const event = makeEvent({ params: { id: '9' }, body: {}, cookies })

    const result = await route.default(event)

    expect(result).toEqual({ paid: true })
    expect(await storedSession(cookies)).toMatchObject({ accessToken: 'fresh', refreshToken: 'r' })
  })

  it('rethrows a refusal with the backend sentence', async () => {
    backend.respondWith(() => ({ status: 400, body: { type: 'INVOICE_ALREADY_PAID', detail: 'Already paid.' } }))

    const error = await refusal(async () => route.default(makeEvent({ params: { id: '9' }, body: {}, cookies: await signedIn('jwt') })))

    expect(error).toMatchObject({ statusCode: 400, statusMessage: 'Already paid.', data: { code: 'INVOICE_ALREADY_PAID' } })
  })
})
