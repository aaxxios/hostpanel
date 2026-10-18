/**
 * Tests for `POST /api/portal/auth/sso/logout`: it clears the session cookies and sends the browser
 * to the SSO end-session endpoint, which returns to the portal.
 *
 * @module tests/server/api/portal/auth/sso/logout.post.test
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
  route = await import('~/server/api/portal/auth/sso/logout.post')
})

afterEach(async () => {
  await backend.close()
})

describe('POST /api/portal/auth/sso/logout', () => {
  it('clears the session cookies and redirects to the end-session endpoint with the portal address', async () => {
    setRuntimeConfig({ public: { ssoPublicUrl: 'https://sso.test', baseUrl: 'https://portal.test' } })
    const event = makeEvent()

    await route.default(event)

    expect(event.deleted).toEqual(['auth_token', 'refresh_token', 'authed'])
    expect(event.redirectedTo).toBe('https://sso.test/connect/endsession?post_logout_redirect_uri=https%3A%2F%2Fportal.test')
  })

  it('returns to the local development address when no base URL is configured', async () => {
    setRuntimeConfig({ public: { ssoPublicUrl: 'https://sso.test' } })
    const event = makeEvent()

    await route.default(event)

    expect(event.redirectedTo).toBe('https://sso.test/connect/endsession?post_logout_redirect_uri=http%3A%2F%2Flocalhost%3A3001')
  })
})
