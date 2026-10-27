/**
 * Server middleware — silent SSO
 *
 * Runs on every request before the page renders.
 * If the user has no `authed` cookie and we haven't tried silent SSO yet,
 * redirects to SSO with prompt=none (no login screen shown).
 *
 * SSO responds:
 *   - Active session  → issues code → callback sets authed → back to same page (logged in, no flash)
 *   - No session      → login_required error → callback returns to the page with the sign-in
 *                        prompt flag, so the page offers the sign-in instead of rendering as a silent guest
 *
 * An `authuser` query on the page (the account the visitor arrived for) is handed to the probe, which
 * passes it to the identity provider as `login_hint`.
 *
 * @module server/middleware/sso-silent
 */

import { LOGIN_HINT_QUERY, SSO_AUTHORIZE_PATH } from '~/constants/clientRoutes'
import { AuthMode } from '~/types/authmode'
import { SessionCookie } from '~/constants/sessionCookie'
import { SSO_SILENT_COOKIE_MAX_AGE } from '~/server/utils/auth-cookie'

/** Sends a signed-out page navigation through one silent SSO probe. */
export default defineEventHandler((event) => {
  // Local-auth mode has its own login flow (client-auth.ts middleware) — never applicable to SSO.
  const config = useRuntimeConfig(event)
  if (config.authMode !== AuthMode.Sso) {
    return
  }

  // Only intercept page navigations (not API calls, assets, etc.)
  const url = event.path ?? ''
  // The file test looks at the path only: an `?authuser=` address in the query has dots too.
  const pathname = url.split('?')[0] ?? ''
  if (
    url.startsWith('/api/') ||
    url.startsWith('/_nuxt/') ||
    url.startsWith('/__nuxt') ||
    pathname.includes('.')
  ) {
    return
  }

  const authed = getCookie(event, SessionCookie.Authed)
  const silentTried = getCookie(event, SessionCookie.SsoSilentTried)

  if (authed || silentTried) {
    return
  }

  // Store the current URL to return to after silent SSO
  setCookie(event, SessionCookie.SsoSilentReturn, url || '/', {
    httpOnly: true,
    sameSite: 'lax',
    maxAge: SSO_SILENT_COOKIE_MAX_AGE,
    path: '/',
  })

  // Mark that we've tried — prevents redirect loop when SSO returns login_required
  setCookie(event, SessionCookie.SsoSilentTried, '1', {
    httpOnly: true,
    sameSite: 'lax',
    maxAge: SSO_SILENT_COOKIE_MAX_AGE,
    path: '/',
  })

  const probe = new URLSearchParams({ prompt: 'none' })
  const loginHint = getQuery(event)[LOGIN_HINT_QUERY]
  if (typeof loginHint === 'string' && loginHint) {
    probe.set(LOGIN_HINT_QUERY, loginHint)
  }
  return sendRedirect(event, `${SSO_AUTHORIZE_PATH}?${probe}`, 302)
})
