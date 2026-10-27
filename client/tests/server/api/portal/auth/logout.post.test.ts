/**
 * Tests for `POST /api/portal/auth/logout`: the session always ends, on the server (the stored
 * tokens are deleted) and in the browser, whether or not the backend could revoke the refresh token.
 *
 * @module tests/server/api/portal/auth/logout.post.test
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { makeEvent, setRuntimeConfig, signedIn, startBackend, storedSession } from '../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../helpers/bffHarness'
import type { StubServer } from '../../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

/** The platform fetch, put back after a test stands in its own (the other harness globals must stay). */
const realFetch = globalThis.fetch

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/auth/logout.post')
})

afterEach(async () => {
  vi.stubGlobal('fetch', realFetch)
  await backend.close()
})

/**
 * Finds the backchannel logout request among the calls a fetch spy saw (the backend revocation
 * goes through the same global).
 *
 * @param spy - The stood-in fetch.
 * @returns The first backchannel URL, or undefined when none was made.
 */
const backchannelTarget = (spy: ReturnType<typeof vi.fn>): string | undefined =>
  (spy.mock.calls as unknown[][]).map((call) => String(call[0])).find((url) => url.includes('logout-redirect'))

describe('POST /api/portal/auth/logout', () => {
  it('revokes the refresh token on the backend, deletes the stored session and clears every auth cookie', async () => {
    const cookies = await signedIn('at', { refreshToken: 'r-1' })
    const event = makeEvent({ cookies })

    const result = await route.default(event)

    expect(result).toEqual({ ok: true })
    expect(backend.requests[0]?.url).toBe('/api/auth/logout')
    expect(backend.requests[0]?.headers['cookie']).toBe('refreshToken=r-1')
    expect(await storedSession(cookies)).toBeNull()
    expect(event.deleted).toEqual(expect.arrayContaining(['portal_session', 'auth_token', 'refresh_token', 'authed']))
  })

  it('sends the backchannel call to the configured main portal with this portal own URL', async () => {
    const fetchSpy = vi.fn(() => Promise.resolve(new Response(null)))
    vi.stubGlobal('fetch', fetchSpy)
    setRuntimeConfig({ mainApiUrl: 'https://main.test', public: { baseUrl: 'https://panel.test' } })
    const token = `x.${Buffer.from(JSON.stringify({ email: 'a@b.test' })).toString('base64url')}.y`

    await route.default(makeEvent({ cookies: await signedIn(token) }))

    const target = backchannelTarget(fetchSpy)
    expect(target).toBe(`https://main.test/api/portal/auth/sso/logout-redirect?redirect_uri=${encodeURIComponent('https://panel.test')}`)
  })

  it('skips the backchannel call when no main portal is configured', async () => {
    const fetchSpy = vi.fn(() => Promise.resolve(new Response(null)))
    vi.stubGlobal('fetch', fetchSpy)
    const token = `x.${Buffer.from(JSON.stringify({ email: 'a@b.test' })).toString('base64url')}.y`

    const result = await route.default(makeEvent({ cookies: await signedIn(token) }))

    expect(result).toEqual({ ok: true })
    expect(backchannelTarget(fetchSpy)).toBeUndefined()
  })

  it('ends the session even when the backend refuses the revocation', async () => {
    backend.respondWith(() => ({ status: 500, body: '' }))
    const cookies = await signedIn('at', { refreshToken: 'r-1' })
    const event = makeEvent({ cookies })

    const result = await route.default(event)

    expect(result).toEqual({ ok: true })
    expect(await storedSession(cookies)).toBeNull()
    expect(event.deleted).toEqual(expect.arrayContaining(['portal_session', 'auth_token', 'refresh_token', 'authed']))
  })

  it('clears the cookies of a browser whose session is already gone', async () => {
    const event = makeEvent({ cookies: { portal_session: 'unknown-id' } })

    const result = await route.default(event)

    expect(result).toEqual({ ok: true })
    expect(event.deleted).toEqual(expect.arrayContaining(['portal_session', 'authed']))
  })
})
