/**
 * Tests for `GET /api/portal/auth/sso/callback`: it checks the state, exchanges the code with the
 * stored PKCE verifier at the SSO token endpoint, keeps the tokens in the server-side session store
 * (the browser gets one small opaque cookie) and redirects back. An error answer from the SSO
 * returns the visitor with the sign-in prompt flag.
 *
 * @module tests/server/api/portal/auth/sso/callback.get.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, setRuntimeConfig, signedIn, startBackend, storedSession } from '../../../../../helpers/bffHarness'
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
  it('exchanges the code at the token endpoint and keeps the tokens on the server', async () => {
    backend.respondWith(() => ({ body: { access_token: 'at', refresh_token: 'rt', expires_in: 900, token_type: 'Bearer' } }))
    const event = makeEvent({ query: { code: 'c1', state: 'st' }, cookies: flowCookies })

    await route.default(event)

    const form = new URLSearchParams(backend.requests[0]?.body ?? '')
    expect(backend.requests[0]?.url).toBe('/connect/token')
    expect(form.get('grant_type')).toBe('authorization_code')
    expect(form.get('code')).toBe('c1')
    expect(form.get('code_verifier')).toBe('ver')
    expect(form.get('client_id')).toBe('client')
    const sessionId = event.written['portal_session']?.value ?? ''
    expect(await storedSession({ portal_session: sessionId })).toMatchObject({ accessToken: 'at', refreshToken: 'rt' })
    expect(event.written['portal_session']?.options).toMatchObject({ httpOnly: true, sameSite: 'lax', path: '/', maxAge: 30 * 24 * 60 * 60 })
    expect(event.written['auth_token']).toBeUndefined()
    expect(event.written['refresh_token']).toBeUndefined()
    expect(event.written['authed']?.value).toBe('1')
    expect(event.deleted).toEqual(expect.arrayContaining(['oauth_state', 'pkce_verifier', 'sso_silent_tried']))
    expect(event.redirectedTo).toBe('/client/dashboard')
  })

  it('keeps every cookie far below the 4 KB browser limit however large the tokens are', async () => {
    // A refresh token of about 5 KB is what the browser silently dropped when it was a cookie.
    const largeAccess = `h.${Buffer.from(JSON.stringify({ sub: 'person-1', pad: 'a'.repeat(1400) })).toString('base64url')}.s`
    const largeRefresh = 'r'.repeat(5000)
    backend.respondWith(() => ({ body: { access_token: largeAccess, refresh_token: largeRefresh, expires_in: 899, token_type: 'Bearer' } }))
    const event = makeEvent({ query: { code: 'c1', state: 'st' }, cookies: flowCookies })

    await route.default(event)

    for (const [name, cookie] of Object.entries(event.written)) {
      expect(`${name}=${cookie.value}`.length, name).toBeLessThan(200)
    }
    expect(await storedSession({ portal_session: event.written['portal_session']?.value ?? '' }))
      .toMatchObject({ accessToken: largeAccess, refreshToken: largeRefresh, subject: 'person-1' })
  })

  it('ends a session the browser already had, so a sign-in never reuses an id', async () => {
    backend.respondWith(() => ({ body: { access_token: 'at', expires_in: 900, token_type: 'Bearer' } }))
    const previous = await signedIn('old')
    const event = makeEvent({ query: { code: 'c1', state: 'st' }, cookies: { ...flowCookies, ...previous } })

    await route.default(event)

    expect(event.written['portal_session']?.value).not.toBe(previous['portal_session'])
    expect(await storedSession(previous)).toBeNull()
  })

  it('stores no refresh token when the SSO issued none, and returns to the remembered path', async () => {
    backend.respondWith(() => ({ body: { access_token: 'at', expires_in: 900, token_type: 'Bearer' } }))
    const event = makeEvent({ query: { code: 'c1', state: 'st' }, cookies: { ...flowCookies, post_login_redirect: '/client/invoices' } })

    await route.default(event)

    const stored = await storedSession({ portal_session: event.written['portal_session']?.value ?? '' })
    expect(stored?.refreshToken).toBeUndefined()
    expect(event.written['portal_session']?.options).toMatchObject({ maxAge: 900 })
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

  it('returns a guest to the page they came from with the sign-in prompt when the SSO answers login_required', async () => {
    const event = makeEvent({ query: { error: 'login_required' }, cookies: { sso_silent_return: '/pricing' } })

    await route.default(event)

    expect(event.redirectedTo).toBe('/pricing?signin=required')
    expect(event.deleted).toEqual(expect.arrayContaining(['pkce_verifier', 'oauth_state', 'sso_silent_return']))
    expect(event.written['portal_session']).toBeUndefined()
    expect(backend.requests).toHaveLength(0)
  })

  it('keeps the account hint of the page it returns to', async () => {
    const event = makeEvent({ query: { error: 'login_required' }, cookies: { sso_silent_return: '/hosting?authuser=a%40b.test' } })

    await route.default(event)

    expect(event.redirectedTo).toBe('/hosting?authuser=a%40b.test&signin=required')
  })

  it('returns to the home page with the prompt when no page was remembered, or the remembered one is not local', async () => {
    const bare = makeEvent({ query: { error: 'login_required' } })
    const foreign = makeEvent({ query: { error: 'login_required' }, cookies: { post_login_redirect: '//evil.test/' } })

    await route.default(bare)
    await route.default(foreign)

    expect(bare.redirectedTo).toBe('/?signin=required')
    expect(foreign.redirectedTo).toBe('/?signin=required')
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

describe('GET /api/portal/auth/sso/callback when the SSO address is not configured', () => {
  it('refuses with 503 and calls no token endpoint when the callback address is empty', async () => {
    setRuntimeConfig({ ssoCallbackUrl: '' })
    const event = makeEvent({ query: { code: 'c1', state: 'st' }, cookies: flowCookies })

    const error = await refusal(() => route.default(event))

    expect(error).toMatchObject({ statusCode: 503, data: { code: 'SSO_NOT_CONFIGURED' } })
    expect(event.redirectedTo).toBeUndefined()
    expect(backend.requests).toHaveLength(0)
  })
})
