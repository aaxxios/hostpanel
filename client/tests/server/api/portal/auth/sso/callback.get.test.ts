/**
 * Tests for `GET /api/portal/auth/sso/callback`: it checks the state, exchanges the code with the
 * stored PKCE verifier at the SSO token endpoint, sets the session cookies and redirects back.
 *
 * @module tests/server/api/portal/auth/sso/callback.get.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, setRuntimeConfig, startBackend } from '../../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../../helpers/bffHarness'
import type { StubServer } from '../../../../../helpers/stubServer'

/** The stub playing the SSO token endpoint. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

/** Cookies a sign-in in progress carries. */
const flowCookies = { oauth_state: 'st', pkce_verifier: 'ver' }

beforeEach(async () => {
  backend = await startBackend()
  setRuntimeConfig({ ssoCallbackUrl: 'https://portal.test/api/portal/auth/sso/callback' })
  route = await import('~/server/api/portal/auth/sso/callback.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/auth/sso/callback', () => {
  it('exchanges the code at the token endpoint and sets the session cookies', async () => {
    backend.respondWith(() => ({ body: { access_token: 'at', refresh_token: 'rt', expires_in: 900, token_type: 'Bearer' } }))
    const event = makeEvent({ query: { code: 'c1', state: 'st' }, cookies: flowCookies })

    await route.default(event)

    const form = new URLSearchParams(backend.requests[0]?.body ?? '')
    expect(backend.requests[0]?.url).toBe('/connect/token')
    expect(form.get('grant_type')).toBe('authorization_code')
    expect(form.get('code')).toBe('c1')
    expect(form.get('code_verifier')).toBe('ver')
    expect(form.get('client_id')).toBe('client')
    expect(event.written['auth_token']?.value).toBe('at')
    expect(event.written['refresh_token']?.value).toBe('rt')
    expect(event.written['authed']?.value).toBe('1')
    expect(event.deleted).toEqual(expect.arrayContaining(['oauth_state', 'pkce_verifier', 'sso_silent_tried']))
    expect(event.redirectedTo).toBe('/client/dashboard')
  })

  it('sets no refresh cookie when the SSO issued none, and returns to the remembered path', async () => {
    backend.respondWith(() => ({ body: { access_token: 'at', expires_in: 900, token_type: 'Bearer' } }))
    const event = makeEvent({ query: { code: 'c1', state: 'st' }, cookies: { ...flowCookies, post_login_redirect: '/client/invoices' } })

    await route.default(event)

    expect(event.written['refresh_token']).toBeUndefined()
    expect(event.redirectedTo).toBe('/client/invoices')
  })

  it('prefers the silent-probe return path over the remembered one', async () => {
    backend.respondWith(() => ({ body: { access_token: 'at', expires_in: 900, token_type: 'Bearer' } }))
    const event = makeEvent({
      query: { code: 'c1', state: 'st' },
      cookies: { ...flowCookies, sso_silent_return: '/pricing', post_login_redirect: '/client/invoices' },
    })

    await route.default(event)

    expect(event.redirectedTo).toBe('/pricing')
  })

  it('returns a guest to the page they came from when the SSO answers with an error', async () => {
    const event = makeEvent({ query: { error: 'login_required' }, cookies: { sso_silent_return: '/pricing' } })

    await route.default(event)

    expect(event.redirectedTo).toBe('/pricing')
    expect(event.deleted).toEqual(expect.arrayContaining(['pkce_verifier', 'oauth_state', 'sso_silent_return']))
    expect(backend.requests).toHaveLength(0)
  })

  it('refuses a callback without a code', async () => {
    const error = await refusal(() => route.default(makeEvent({ query: {}, cookies: flowCookies })))

    expect(error).toMatchObject({ statusCode: 400, data: { code: 'OAUTH_CODE_MISSING' } })
  })

  it('refuses a callback whose state does not match the stored one', async () => {
    const error = await refusal(() => route.default(makeEvent({ query: { code: 'c1', state: 'other' }, cookies: flowCookies })))

    expect(error).toMatchObject({ statusCode: 400, data: { code: 'OAUTH_STATE_INVALID' } })
    expect(backend.requests).toHaveLength(0)
  })

  it('refuses a callback without the PKCE verifier cookie', async () => {
    const error = await refusal(() => route.default(makeEvent({ query: { code: 'c1', state: 'st' }, cookies: { oauth_state: 'st' } })))

    expect(error).toMatchObject({ statusCode: 400, data: { code: 'OAUTH_VERIFIER_MISSING' } })
  })

  it('passes the SSO refusal code through when the token exchange fails', async () => {
    backend.respondWith(() => ({ status: 400, body: { error: 'invalid_grant' } }))

    const error = await refusal(() => route.default(makeEvent({ query: { code: 'c1', state: 'st' }, cookies: flowCookies })))

    expect(error).toMatchObject({ statusCode: 401, data: { code: 'invalid_grant' } })
  })

  it('falls back to a generic code when the SSO sends no error body', async () => {
    backend.respondWith(() => ({ status: 500, body: 'boom' }))

    const error = await refusal(() => route.default(makeEvent({ query: { code: 'c1', state: 'st' }, cookies: flowCookies })))

    expect(error).toMatchObject({ statusCode: 401, data: { code: 'TOKEN_EXCHANGE_FAILED' } })
  })
})
