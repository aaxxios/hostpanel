/**
 * Tests for `GET /api/portal/auth/sso/switch`: switching account only starts the external sign-on
 * authorize flow (state and PKCE bound to the browser in httpOnly cookies) through the provider's
 * switch-init page, which receives the account and a relative authorize address;
 * the local session is never touched here, only by the callback after verification.
 *
 * @module tests/server/api/portal/auth/sso/switch.get.test
 */

import { createHash } from 'node:crypto'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, setRuntimeConfig, startBackend, refusal } from '../../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../../helpers/bffHarness'
import type { StubServer } from '../../../../../helpers/stubServer'

/** The stub standing in for the external sign-on (the route only redirects to it). */
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
  route = await import('~/server/api/portal/auth/sso/switch.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/auth/sso/switch', () => {
  it('redirects to the provider switch-init page with the account and a relative authorize address bound to the browser', async () => {
    const event = makeEvent({ query: { sub: 'u 2', redirect: '/client/domains' } })

    await route.default(event)

    const target = new URL(event.redirectedTo ?? '')
    expect(target.origin + target.pathname).toBe('https://sso.test/switch-init')
    expect(event.redirectedTo).toContain('sub=u%202&')
    expect(target.searchParams.get('sub')).toBe('u 2')
    const returnUrl = target.searchParams.get('returnUrl') ?? ''
    expect(returnUrl.startsWith('/connect/authorize?')).toBe(true)
    const authorize = new URLSearchParams(returnUrl.slice(returnUrl.indexOf('?') + 1))
    expect(authorize.get('client_id')).toBe('client')
    expect(authorize.get('response_type')).toBe('code')
    expect(authorize.get('redirect_uri')).toBe('https://portal.test/api/portal/auth/sso/callback')
    expect(authorize.get('scope')).toBe('openid profile email offline_access')
    expect(authorize.get('code_challenge_method')).toBe('S256')
    expect(authorize.has('prompt')).toBe(false)
    expect(authorize.has('login_hint')).toBe(false)
    expect(authorize.get('state')).toBe(event.written['oauth_state']?.value)
    const verifier = event.written['pkce_verifier']?.value ?? ''
    expect(authorize.get('code_challenge')).toBe(createHash('sha256').update(verifier).digest('base64url'))
    expect(event.written['pkce_verifier']?.options).toMatchObject({ httpOnly: true, secure: true, sameSite: 'lax', maxAge: 300 })
    expect(event.written['oauth_state']?.options).toMatchObject({ httpOnly: true, secure: true, sameSite: 'lax', maxAge: 300 })
    expect(event.written['post_login_redirect']?.value).toBe('/client/domains')
  })

  it('does not touch the local session: it writes only the flow cookies and deletes none', async () => {
    const event = makeEvent({ query: { sub: 'u-2' }, cookies: { portal_session: 'current' } })

    await route.default(event)

    expect(Object.keys(event.written).sort()).toEqual(['oauth_state', 'pkce_verifier', 'post_login_redirect'])
    expect(event.deleted).toEqual([])
  })

  it('returns to the client area after the switch when no redirect is given', async () => {
    const event = makeEvent({ query: { sub: 'u-2' } })

    await route.default(event)

    expect(event.written['post_login_redirect']?.value).toBe('/client')
  })

  it.each(['https://elsewhere.test/x', '//elsewhere.test', '/\\elsewhere.test', '/api/portal/x'])(
    'keeps the client area instead of the refused redirect %s',
    async (redirect) => {
      const event = makeEvent({ query: { sub: 'u-2', redirect } })

      await route.default(event)

      expect(event.written['post_login_redirect']?.value).toBe('/client')
    },
  )

  it('falls back to the plain authorize route when no account is named', async () => {
    const event = makeEvent({ query: {} })

    await route.default(event)

    expect(event.redirectedTo).toBe('/api/portal/auth/sso/authorize')
    expect(event.written).toEqual({})
  })

  it.each([[{ sub: ['a', 'b'] }], [{ sub: 'u-2', redirect: ['/x', '/y'] }]])('refuses the repeated key in %j with 400 and sets no cookie', async (query) => {
    const event = makeEvent({ query: query as unknown as Record<string, string> })

    const error = await refusal(() => route.default(event))

    expect(error).toMatchObject({ statusCode: 400, data: { code: 'BAD_QUERY' } })
    expect(event.redirectedTo).toBeUndefined()
    expect(event.written).toEqual({})
  })
})

describe('GET /api/portal/auth/sso/switch when the sign-on address is not configured', () => {
  it('refuses with 503 and sets no cookie when the public address is empty', async () => {
    setRuntimeConfig({ ssoCallbackUrl: 'https://portal.test/api/portal/auth/sso/callback', public: { ssoPublicUrl: '' } })
    const event = makeEvent({ query: { sub: 'u2' } })

    const error = await refusal(() => route.default(event))

    expect(error).toMatchObject({ statusCode: 503, data: { code: 'SSO_NOT_CONFIGURED' } })
    expect(event.written).toEqual({})
  })

  it('refuses with 503 when the callback address is empty', async () => {
    setRuntimeConfig({ ssoCallbackUrl: '', public: { ssoPublicUrl: 'https://sso.test' } })
    const event = makeEvent({ query: { sub: 'u2' } })

    const error = await refusal(() => route.default(event))

    expect(error).toMatchObject({ statusCode: 503, data: { code: 'SSO_NOT_CONFIGURED' } })
    expect(event.written).toEqual({})
  })
})
