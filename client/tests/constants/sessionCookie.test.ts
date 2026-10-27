/**
 * Unit tests for `constants/sessionCookie.ts`.
 *
 * @module tests/constants/sessionCookie.test
 */

import { describe, expect, it } from 'vitest'
import { SessionCookie } from '~/constants/sessionCookie'

describe('SessionCookie', () => {
  it('keeps the wire names of the session cookies', () => {
    expect(SessionCookie.Session).toBe('portal_session')
    expect(SessionCookie.Authed).toBe('authed')
  })

  it('keeps the wire names of the legacy token cookies it still clears', () => {
    expect(SessionCookie.LegacyAuthToken).toBe('auth_token')
    expect(SessionCookie.LegacyRefreshToken).toBe('refresh_token')
  })

  it('keeps the wire names of the sign-in round-trip cookies', () => {
    expect(SessionCookie.PkceVerifier).toBe('pkce_verifier')
    expect(SessionCookie.OauthState).toBe('oauth_state')
    expect(SessionCookie.PostLoginRedirect).toBe('post_login_redirect')
    expect(SessionCookie.SsoSilentTried).toBe('sso_silent_tried')
    expect(SessionCookie.SsoSilentReturn).toBe('sso_silent_return')
  })

  it('keeps the wire names of the legacy account cookie and the client session', () => {
    expect(SessionCookie.PendingAccount).toBe('inno_pending_account')
    expect(SessionCookie.ClientSession).toBe('client_session')
  })

  it('gives every cookie a distinct name', () => {
    const names = Object.values(SessionCookie)
    expect(new Set(names).size).toBe(names.length)
  })
})
