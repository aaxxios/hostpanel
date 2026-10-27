/**
 * Tests for `POST /api/portal/auth/sso/logout`: it ends the server-side session (stored tokens and
 * cookies) and sends the browser
 * to the SSO end-session endpoint, which returns to the portal.
 *
 * @module tests/server/api/portal/auth/sso/logout.post.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, setRuntimeConfig, signedIn, startBackend, storedSession, refusal } from '../../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../../helpers/bffHarness'
import type { StubServer } from '../../../../../helpers/stubServer'

/** The stub standing in for the SSO (the route only redirects to it). */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/auth/sso/logout.post')
})

afterEach(async () => {
  await backend.close()
})

describe('POST /api/portal/auth/sso/logout', () => {
  it('ends the session and redirects to the end-session endpoint with the portal address', async () => {
    setRuntimeConfig({ public: { ssoPublicUrl: 'https://sso.test', baseUrl: 'https://portal.test' } })
    const cookies = await signedIn('at', { refreshToken: 'rt' })
    const event = makeEvent({ cookies })

    await route.default(event)

    expect(await storedSession(cookies)).toBeNull()
    expect(event.deleted).toEqual(expect.arrayContaining(['portal_session', 'auth_token', 'refresh_token', 'authed']))
    expect(event.redirectedTo).toBe('https://sso.test/connect/endsession?post_logout_redirect_uri=https%3A%2F%2Fportal.test')
  })

  it('returns to the origin the request arrived on when no base URL is configured', async () => {
    setRuntimeConfig({ public: { ssoPublicUrl: 'https://sso.test' } })
    const event = makeEvent({ headers: { host: 'portal.test' } })

    await route.default(event)

    expect(event.redirectedTo).toBe('https://sso.test/connect/endsession?post_logout_redirect_uri=http%3A%2F%2Fportal.test')
  })
})

describe('POST /api/portal/auth/sso/logout when the SSO address is not configured', () => {
  it('refuses with 503 and keeps the session cookies when the public SSO address is empty', async () => {
    setRuntimeConfig({ public: { ssoPublicUrl: '' } })
    const event = makeEvent()

    const error = await refusal(() => route.default(event))

    expect(error).toMatchObject({ statusCode: 503, data: { code: 'SSO_NOT_CONFIGURED' } })
    expect(event.redirectedTo).toBeUndefined()
    expect(event.deleted).toEqual([])
  })
})
