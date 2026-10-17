/**
 * Tests for `GET /api/portal/client/me`: in single sign-on mode it merges the SSO identity with the
 * billing profile, in local mode it maps the billing profile alone.
 *
 * @module tests/server/api/portal/client/me.get.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, setRuntimeConfig, startBackend } from '../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../helpers/bffHarness'
import type { StubReply, StubRequest, StubServer } from '../../../../helpers/stubServer'
import { AuthMode } from '~/types/authmode'

/** The stub playing both the SSO profile endpoint and the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

/** The billing profile the C# API answers with. */
const billing = {
  id: 'b-1',
  firstName: 'Ann',
  lastName: 'Lee',
  companyName: 'Acme',
  email: 'ann@b.test',
  phone: '+1',
  street: 'Main 1',
  address2: 'Apt 2',
  city: 'Yerevan',
  state: 'AR',
  postCode: '0001',
  country: 'AM',
  paymentMethod: 'card',
  language: 'hy',
  currency: 'USD',
  notifyGeneral: true,
  notifyInvoice: false,
  notifySupport: true,
  notifyProduct: false,
  notifyDomain: true,
  notifyAffiliate: false,
  twoFactorEnabled: true,
}

/**
 * Answers the SSO profile and the billing profile by path.
 *
 * @param request - The request the stub received.
 * @returns The reply for that path.
 */
const answer = (request: StubRequest): StubReply => {
  if (request.url === '/api/account/profile') {
    return { body: { id: 'sso-1', firstName: 'Id', lastName: 'Entity', email: 'sso@b.test', phoneNumber: '+9', totpEnabled: true } }
  }
  return { body: billing }
}

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/client/me.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/client/me in single sign-on mode', () => {
  beforeEach(() => {
    setRuntimeConfig({ authMode: AuthMode.Sso })
    backend.respondWith(answer)
  })

  it('merges identity and billing, preferring billing names, and forwards the language to the billing call', async () => {
    const result = await route.default(makeEvent({ cookies: { auth_token: 'jwt' }, headers: { 'x-language': 'ru' } }))

    expect(backend.requests[0]?.headers['authorization']).toBe('Bearer jwt')
    expect(backend.requests[1]?.url).toBe('/api/clients/me')
    expect(backend.requests[1]?.headers['x-language']).toBe('ru')
    expect(result).toMatchObject({
      id: 'sso-1',
      firstname: 'Ann',
      lastname: 'Lee',
      companyname: 'Acme',
      email: 'sso@b.test',
      phonenumber: '+1',
      postcode: '0001',
      defaultgateway: 'card',
      currency: 'USD',
      twoFactorEnabled: true,
      email_preferences: { general: 1, invoice: 0, support: 1, product: 0, domain: 1, affiliate: 0 },
    })
  })

  it('falls back to the identity when the billing profile is refused', async () => {
    backend.respondWith(request => (request.url === '/api/account/profile' ? answer(request) : { status: 404, body: { type: 'CLIENT_PROFILE_NOT_FOUND', detail: 'None.' } }))

    const result = await route.default(makeEvent({ cookies: { auth_token: 'jwt' } })) as Record<string, unknown>

    expect(result['firstname']).toBe('Id')
    expect(result['phonenumber']).toBe('+9')
    expect(result['email_preferences']).toBeNull()
    expect(result['currency']).toBeNull()
  })

  it('refuses with 401 without calling anything when there is no session', async () => {
    const error = await refusal(() => route.default(makeEvent()))

    expect(error).toMatchObject({ statusCode: 401 })
    expect(backend.requests).toHaveLength(0)
  })

  it('refuses with the SSO status when the profile call fails', async () => {
    backend.respondWith(() => ({ status: 403, body: {} }))

    const error = await refusal(() => route.default(makeEvent({ cookies: { auth_token: 'jwt' } })))

    expect(error).toMatchObject({ statusCode: 403 })
  })
})

describe('GET /api/portal/client/me in local mode', () => {
  beforeEach(() => {
    setRuntimeConfig({ authMode: AuthMode.Local })
  })

  it('maps the billing profile to the portal shape', async () => {
    backend.respondWith(() => ({ body: billing }))

    const result = await route.default(makeEvent({ cookies: { auth_token: 'jwt' } }))

    expect(backend.requests).toHaveLength(1)
    expect(result).toMatchObject({
      id: 'b-1',
      firstname: 'Ann',
      address1: 'Main 1',
      language: 'hy',
      twoFactorEnabled: true,
      email_preferences: { general: 1, invoice: 0, support: 1, product: 0, domain: 1, affiliate: 0 },
    })
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 404, body: { type: 'CLIENT_PROFILE_NOT_FOUND', detail: 'No profile.' } }))

    const error = await refusal(() => route.default(makeEvent({ cookies: { auth_token: 'jwt' } })))

    expect(error).toMatchObject({ statusCode: 404, statusMessage: 'No profile.', data: { code: 'CLIENT_PROFILE_NOT_FOUND' } })
  })
})
