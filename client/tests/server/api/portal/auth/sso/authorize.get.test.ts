/**
 * Tests for `GET /api/portal/auth/sso/authorize`: it starts the authorization-code flow with PKCE,
 * stores the verifier, state and return path in short-lived httpOnly cookies, and redirects to the SSO.
 *
 * @module tests/server/api/portal/auth/sso/authorize.get.test
 */

import { createHash } from 'node:crypto'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, setRuntimeConfig, startBackend, refusal } from '../../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../../helpers/bffHarness'
import type { StubServer } from '../../../../../helpers/stubServer'

/** The stub standing in for the SSO (the route only redirects to it). */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  setRuntimeConfig({
    ssoClientId: 'client',
    ssoCallbackUrl: 'https://portal.test/api/portal/auth/sso/callback',
    public: { ssoPublicUrl: 'https://sso.test' },
  })
  route = await import('~/server/api/portal/auth/sso/authorize.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/auth/sso/authorize', () => {
  it('redirects to the SSO authorize endpoint with a PKCE challenge that matches the stored verifier', async () => {
    const event = makeEvent()

    await route.default(event)

    const target = new URL(event.redirectedTo ?? '')
    expect(target.origin + target.pathname).toBe('https://sso.test/connect/authorize')
    expect(target.searchParams.get('client_id')).toBe('client')
    expect(target.searchParams.get('response_type')).toBe('code')
    expect(target.searchParams.get('redirect_uri')).toBe('https://portal.test/api/portal/auth/sso/callback')
    expect(target.searchParams.get('code_challenge_method')).toBe('S256')
    expect(target.searchParams.get('state')).toBe(event.written['oauth_state']?.value)
    const verifier = event.written['pkce_verifier']?.value ?? ''
    expect(target.searchParams.get('code_challenge')).toBe(createHash('sha256').update(verifier).digest('base64url'))
    expect(event.written['pkce_verifier']?.options).toMatchObject({ httpOnly: true, maxAge: 300 })
  })

  it('adds the prompt parameter when the query carries one', async () => {
    const event = makeEvent({ query: { prompt: 'none' } })

    await route.default(event)

    expect(new URL(event.redirectedTo ?? '').searchParams.get('prompt')).toBe('none')
  })

  it('passes an authuser address on as login_hint', async () => {
    const event = makeEvent({ query: { authuser: ' person@example.test ' } })

    await route.default(event)

    expect(new URL(event.redirectedTo ?? '').searchParams.get('login_hint')).toBe('person@example.test')
  })

  it('sends no login_hint without an authuser, or for one too long to be an address', async () => {
    const plain = makeEvent()
    const oversized = makeEvent({ query: { authuser: `${'a'.repeat(250)}@example.test` } })

    await route.default(plain)
    await route.default(oversized)

    expect(new URL(plain.redirectedTo ?? '').searchParams.has('login_hint')).toBe(false)
    expect(new URL(oversized.redirectedTo ?? '').searchParams.has('login_hint')).toBe(false)
  })

  it('remembers a local returnTo path', async () => {
    const event = makeEvent({ query: { returnTo: '/client/invoices' } })

    await route.default(event)

    expect(event.written['post_login_redirect']?.value).toBe('/client/invoices')
  })

  it('falls back to the referer path when no returnTo is given', async () => {
    const event = makeEvent({ headers: { referer: 'https://portal.test/client/tickets?x=1' } })

    await route.default(event)

    expect(event.written['post_login_redirect']?.value).toBe('/client/tickets')
  })

  it('refuses to remember an api path or an off-site address as the return path', async () => {
    const api = makeEvent({ query: { returnTo: '/api/portal/x' } })
    const offSite = makeEvent({ query: { returnTo: 'https://evil.test/x' } })

    await route.default(api)
    await route.default(offSite)

    expect(api.written['post_login_redirect']).toBeUndefined()
    expect(offSite.written['post_login_redirect']).toBeUndefined()
  })
})

describe('GET /api/portal/auth/sso/authorize when the SSO address is not configured', () => {
  it('refuses with 503 and sets no cookie when the public SSO address is empty', async () => {
    setRuntimeConfig({ ssoCallbackUrl: 'https://portal.test/api/portal/auth/sso/callback', public: { ssoPublicUrl: '' } })
    const event = makeEvent()

    const error = await refusal(() => route.default(event))

    expect(error).toMatchObject({ statusCode: 503, data: { code: 'SSO_NOT_CONFIGURED' } })
    expect(event.redirectedTo).toBeUndefined()
    expect(event.written).toEqual({})
  })

  it('refuses with 503 when the callback address is empty', async () => {
    setRuntimeConfig({ ssoCallbackUrl: '', public: { ssoPublicUrl: 'https://sso.test' } })
    const event = makeEvent()

    const error = await refusal(() => route.default(event))

    expect(error).toMatchObject({ statusCode: 503, data: { code: 'SSO_NOT_CONFIGURED' } })
    expect(event.redirectedTo).toBeUndefined()
  })
})
