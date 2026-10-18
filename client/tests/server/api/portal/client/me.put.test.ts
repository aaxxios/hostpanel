/**
 * Tests for `PUT /api/portal/client/me`: it reads the current profile, overlays only the fields the
 * form posted, and writes the full profile back.
 *
 * @module tests/server/api/portal/client/me.put.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend } from '../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../helpers/bffHarness'
import type { StubServer } from '../../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

/** The profile the backend currently holds. */
const current = {
  email: 'old@b.test',
  firstName: 'Ann',
  lastName: 'Lee',
  companyName: 'Acme',
  phone: '+1',
  street: 'Main 1',
  address2: '',
  city: 'Yerevan',
  state: '',
  postCode: '0001',
  country: 'AM',
  language: 'hy',
  currency: 'USD',
  paymentMethod: 'card',
  notifyGeneral: true,
  notifyInvoice: true,
  taxExempt: false,
}

/**
 * Parses the body of the `PUT` the stub received.
 *
 * @returns The written profile.
 */
const written = (): Record<string, unknown> => {
  const put = backend.requests.find(request => request.method === 'PUT')
  return JSON.parse(put?.body ?? '{}') as Record<string, unknown>
}

beforeEach(async () => {
  backend = await startBackend((request) => (request.method === 'GET' ? { body: current } : { body: { saved: true } }))
  route = await import('~/server/api/portal/client/me.put')
})

afterEach(async () => {
  await backend.close()
})

describe('PUT /api/portal/client/me', () => {
  it('overlays the posted fields on the current profile and writes it back with the bearer and language', async () => {
    const result = await route.default(makeEvent({
      body: { firstname: 'Anna', emailprefs: { invoice: false } },
      cookies: { auth_token: 'jwt' },
      headers: { 'x-language': 'ru' },
    }))

    expect(result).toEqual({ saved: true })
    expect(backend.requests.map(request => `${request.method} ${request.url}`)).toEqual(['GET /api/clients/me', 'PUT /api/clients/me'])
    expect(backend.requests[1]?.headers['authorization']).toBe('Bearer jwt')
    expect(backend.requests[1]?.headers['x-language']).toBe('ru')
    expect(written()).toMatchObject({
      FirstName: 'Anna',
      LastName: 'Lee',
      Email: 'old@b.test',
      PostCode: '0001',
      NotifyInvoice: false,
      NotifyGeneral: true,
      Currency: 'USD',
      TaxExempt: false,
      Status: null,
    })
  })

  it('turns an empty language into null and keeps the current one when none is posted', async () => {
    await route.default(makeEvent({ body: { language: '' }, cookies: { auth_token: 'jwt' } }))
    const cleared = written()
    backend.requests.length = 0
    await route.default(makeEvent({ body: {}, cookies: { auth_token: 'jwt' } }))

    expect(cleared['Language']).toBeNull()
    expect(written()['Language']).toBe('hy')
  })

  it('passes the backend refusal of the write through with its code, sentence and field errors', async () => {
    backend.respondWith(request => (request.method === 'GET'
      ? { body: current }
      : { status: 400, body: { type: 'VALIDATION_ERROR', detail: 'Invalid.', errors: { Email: ['Required.'] } } }))

    const error = await refusal(() => route.default(makeEvent({ body: { email: '' }, cookies: { auth_token: 'jwt' } })))

    expect(error).toMatchObject({ statusCode: 400, statusMessage: 'Invalid.', data: { code: 'VALIDATION_ERROR', errors: { Email: ['Required.'] } } })
  })

  it('does not write when reading the current profile is refused', async () => {
    backend.respondWith(() => ({ status: 404, body: { type: 'CLIENT_PROFILE_NOT_FOUND', detail: 'No profile.' } }))

    const error = await refusal(() => route.default(makeEvent({ body: {}, cookies: { auth_token: 'jwt' } })))

    expect(error).toMatchObject({ statusCode: 404, data: { code: 'CLIENT_PROFILE_NOT_FOUND' } })
    expect(backend.requests.some(request => request.method === 'PUT')).toBe(false)
  })
})
