/**
 * Tests for the silent single-sign-on middleware: a signed-out page navigation in SSO mode is sent
 * through one `prompt=none` probe, once, and nothing else is touched.
 *
 * @module tests/server/middleware/sso-silent.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, setRuntimeConfig, startBackend } from '../../helpers/bffHarness'
import type { MiddlewareModule } from '../../helpers/bffHarness'
import type { StubServer } from '../../helpers/stubServer'
import { AuthMode } from '~/types/authmode'

/** The stub the harness points the runtime config at (the middleware itself calls nothing). */
let backend: StubServer

/** The middleware under test. */
let middleware: MiddlewareModule

beforeEach(async () => {
  backend = await startBackend()
  setRuntimeConfig({ authMode: AuthMode.Sso })
  middleware = await import('~/server/middleware/sso-silent')
})

afterEach(async () => {
  await backend.close()
})

describe('sso-silent middleware', () => {
  it('redirects a signed-out page navigation to the silent authorize probe and remembers where it was going', () => {
    const event = makeEvent({ path: '/client/dashboard' })

    middleware.default(event)

    expect(event.redirectedTo).toBe('/api/portal/auth/sso/authorize?prompt=none')
    expect(event.written['sso_silent_return']).toMatchObject({ value: '/client/dashboard', options: { httpOnly: true, maxAge: 30 } })
    expect(event.written['sso_silent_tried']).toMatchObject({ value: '1', options: { httpOnly: true, maxAge: 30 } })
  })

  it('does nothing in local authentication mode', () => {
    setRuntimeConfig({ authMode: AuthMode.Local })
    const event = makeEvent({ path: '/client/dashboard' })

    middleware.default(event)

    expect(event.redirectedTo).toBeUndefined()
    expect(event.written).toEqual({})
  })

  it('leaves api calls, build assets and files alone', () => {
    for (const path of ['/api/portal/client/me', '/_nuxt/entry.js', '/__nuxt_error', '/favicon.ico']) {
      const event = makeEvent({ path })

      middleware.default(event)

      expect(event.redirectedTo).toBeUndefined()
      expect(event.written).toEqual({})
    }
  })

  it('does nothing for a signed-in visitor', () => {
    const event = makeEvent({ path: '/client/dashboard', cookies: { authed: '1' } })

    middleware.default(event)

    expect(event.redirectedTo).toBeUndefined()
  })

  it('does not probe a second time once the probe was tried, so a login_required answer cannot loop', () => {
    const event = makeEvent({ path: '/client/dashboard', cookies: { sso_silent_tried: '1' } })

    middleware.default(event)

    expect(event.redirectedTo).toBeUndefined()
    expect(event.written).toEqual({})
  })

  it('hands an authuser address on the page to the probe, even though the address has dots', () => {
    const event = makeEvent({ path: '/hosting?authuser=person%40example.test', query: { authuser: 'person@example.test' } })

    middleware.default(event)

    expect(event.redirectedTo).toBe('/api/portal/auth/sso/authorize?prompt=none&authuser=person%40example.test')
    expect(event.written['sso_silent_return']?.value).toBe('/hosting?authuser=person%40example.test')
  })
})
