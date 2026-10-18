/**
 * Tests for `GET /api/portal/auth/sso/switch`: switching account starts a PKCE flow through the
 * SSO `switch-init` page, or falls back to a plain authorize when no account is named.
 *
 * @module tests/server/api/portal/auth/sso/switch.get.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, setRuntimeConfig, startBackend } from '../../../../../helpers/bffHarness'
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
  route = await import('~/server/api/portal/auth/sso/switch.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/auth/sso/switch', () => {
  it('redirects to the SSO switch-init page carrying the account and an encoded authorize return URL', async () => {
    const event = makeEvent({ query: { sub: 'u 2', redirect: '/client/domains' } })

    await route.default(event)

    const target = new URL(event.redirectedTo ?? '')
    expect(target.origin + target.pathname).toBe('https://sso.test/switch-init')
    expect(target.searchParams.get('sub')).toBe('u 2')
    const returnUrl = new URL(target.searchParams.get('returnUrl') ?? '', 'https://sso.test')
    expect(returnUrl.pathname).toBe('/connect/authorize')
    expect(returnUrl.searchParams.get('client_id')).toBe('client')
    expect(returnUrl.searchParams.get('code_challenge_method')).toBe('S256')
    expect(returnUrl.searchParams.get('state')).toBe(event.written['oauth_state']?.value)
    expect(event.written['pkce_verifier']?.options).toMatchObject({ httpOnly: true, maxAge: 300 })
    expect(event.written['post_login_redirect']?.value).toBe('/client/domains')
  })

  it('returns to the client area after the switch when no redirect is given', async () => {
    const event = makeEvent({ query: { sub: 'u-2' } })

    await route.default(event)

    expect(event.written['post_login_redirect']?.value).toBe('/client')
  })

  it('falls back to the plain authorize route when no account is named', async () => {
    const event = makeEvent()

    await route.default(event)

    expect(event.redirectedTo).toBe('/api/portal/auth/sso/authorize')
    expect(event.written).toEqual({})
  })
})
