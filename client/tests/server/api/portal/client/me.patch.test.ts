/**
 * Tests for `PATCH /api/portal/client/me`: it forwards the camelCase body to the backend as is,
 * with the caller's bearer and language, and passes refusals through.
 *
 * @module tests/server/api/portal/client/me.patch.test
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
  backend = await startBackend(() => ({ body: { saved: true } }))
  route = await import('~/server/api/portal/client/me.patch')
})

afterEach(async () => {
  await backend.close()
})

describe('PATCH /api/portal/client/me', () => {
  it('forwards the body unchanged in one PATCH with the bearer and language', async () => {
    const body = { firstName: 'Anna', address: { city: 'Yerevan' }, language: '' }

    const result = await route.default(makeEvent({
      body,
      cookies: await signedIn('jwt'),
      headers: { 'x-language': 'ru' },
    }))

    expect(result).toEqual({ saved: true })
    expect(backend.requests.map(request => `${request.method} ${request.url}`)).toEqual(['PATCH /api/clients/me'])
    expect(backend.requests[0]?.headers['authorization']).toBe('Bearer jwt')
    expect(backend.requests[0]?.headers['x-language']).toBe('ru')
    expect(JSON.parse(backend.requests[0]?.body ?? '{}')).toEqual(body)
  })

  it('passes the backend refusal through with its code, sentence and field errors', async () => {
    backend.respondWith(() => ({ status: 400, body: { type: 'VALIDATION_ERROR', detail: 'Invalid.', errors: { firstName: ['Required.'] } } }))

    const error = await refusal(async () => route.default(makeEvent({ body: { firstName: '' }, cookies: await signedIn('jwt') })))

    expect(error).toMatchObject({ statusCode: 400, statusMessage: 'Invalid.', data: { code: 'VALIDATION_ERROR', errors: { firstName: ['Required.'] } } })
  })
})
