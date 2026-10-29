/**
 * Tests for `server/utils/portal-session.ts`, over the real in-process session store and a real
 * HTTP stub playing the SSO token endpoint: a sign-in keeps the tokens on the server and gives the
 * browser one small opaque cookie, an expiring access token is renewed with the refresh token
 * before it is used, concurrent renewals share one exchange, and signing out deletes the entry.
 *
 * @module tests/server/utils/portal-session.test
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { makeEvent, refusal, setRuntimeConfig, signedIn, startBackend, storedSession } from '../../helpers/bffHarness'
import type { StubServer } from '../../helpers/stubServer'
import {
  endSession,
  hasSessionCookie,
  readSession,
  refreshSession,
  resolveAccessToken,
  sessionLifetime,
  startSession,
  updateAccessToken
} from '~/server/utils/portal-session'
import { useSessionStore } from '~/server/utils/session-store'

/** The stub playing the SSO token endpoint. */
let sso: StubServer

beforeEach(async () => {
  sso = await startBackend()
})

afterEach(async () => {
  await sso.close()
})

/**
 * Builds an unsigned JWT-shaped token around some claims.
 *
 * @param claims - The claims.
 * @returns The compact token.
 */
const jwt = (claims: Record<string, unknown>): string => `h.${Buffer.from(JSON.stringify(claims)).toString('base64url')}.s`

/**
 * The current time in seconds.
 *
 * @returns Unix time, whole seconds.
 */
const now = (): number => Math.floor(Date.now() / 1000)

describe('startSession', () => {
  it('stores the tokens and their owner under a random id and sets only that id as an httpOnly cookie', async () => {
    const accessToken = jwt({ sub: 'person-1', exp: now() + 900 })
    const event = makeEvent()

    await startSession(event, { accessToken, refreshToken: 'r'.repeat(5000), expiresIn: 900 })

    const cookie = event.written['portal_session']
    expect(cookie?.value).toMatch(/^[A-Za-z0-9_-]{43}$/)
    expect(cookie?.options).toMatchObject({ httpOnly: true, secure: true, sameSite: 'lax', path: '/', maxAge: 30 * 24 * 60 * 60 })
    expect(event.written['authed']).toMatchObject({ value: '1', options: { httpOnly: false, secure: true, sameSite: 'lax', path: '/' } })
    expect(event.deleted).toEqual(expect.arrayContaining(['auth_token', 'refresh_token']))
    expect(await storedSession({ portal_session: cookie?.value ?? '' }))
      .toMatchObject({ accessToken, refreshToken: 'r'.repeat(5000), subject: 'person-1' })
  })

  it('never sets a cookie anywhere near the 4 KB browser limit', async () => {
    const event = makeEvent()

    await startSession(event, { accessToken: jwt({ sub: 'p', pad: 'x'.repeat(2000) }), refreshToken: 'r'.repeat(6000) })

    for (const [name, cookie] of Object.entries(event.written)) {
      expect(`${name}=${cookie.value}`.length, name).toBeLessThan(100)
    }
  })

  it('sizes a session without a refresh token by its access token', async () => {
    const event = makeEvent()

    await startSession(event, { accessToken: 'opaque', expiresIn: 600 })

    expect(event.written['portal_session']?.options).toMatchObject({ maxAge: 600 })
  })
})

describe('sessionLifetime', () => {
  it('is the refresh-token lifetime with a refresh token, the access token remainder without, the fallback otherwise', () => {
    expect(sessionLifetime({ accessToken: 'a', refreshToken: 'r' })).toBe(30 * 24 * 60 * 60)
    expect(sessionLifetime({ accessToken: 'a', accessTokenExpiresAt: now() + 120 })).toBeGreaterThan(110)
    expect(sessionLifetime({ accessToken: 'a', accessTokenExpiresAt: now() - 10 })).toBe(1)
    expect(sessionLifetime({ accessToken: 'a' })).toBe(900)
  })
})

describe('readSession and hasSessionCookie', () => {
  it('reads the session the cookie names, and nothing for an unknown or missing cookie', async () => {
    const cookies = await signedIn('at')

    expect((await readSession(makeEvent({ cookies })))?.session).toMatchObject({ accessToken: 'at' })
    expect(await readSession(makeEvent({ cookies: { portal_session: 'unknown' } }))).toBeNull()
    expect(await readSession(makeEvent())).toBeNull()
    expect(hasSessionCookie(makeEvent({ cookies }))).toBe(true)
    expect(hasSessionCookie(makeEvent())).toBe(false)
  })
})

describe('resolveAccessToken', () => {
  it('answers the stored access token while it is good', async () => {
    const cookies = await signedIn('at', { refreshToken: 'rt', accessTokenExpiresAt: now() + 600 })

    expect(await resolveAccessToken(makeEvent({ cookies }))).toBe('at')
    expect(sso.requests).toHaveLength(0)
  })

  it('renews an access token that is about to expire before using it', async () => {
    sso.respondWith(() => ({ body: { access_token: 'fresh', refresh_token: 'rt-2', expires_in: 900 } }))
    const cookies = await signedIn('at', { refreshToken: 'rt', subject: 'person-1', accessTokenExpiresAt: now() + 5 })
    const event = makeEvent({ cookies })

    expect(await resolveAccessToken(event)).toBe('fresh')

    const form = new URLSearchParams(sso.requests[0]?.body ?? '')
    expect(sso.requests[0]?.url).toBe('/connect/token')
    expect(form.get('grant_type')).toBe('refresh_token')
    expect(form.get('refresh_token')).toBe('rt')
    expect(await storedSession(cookies)).toMatchObject({ accessToken: 'fresh', refreshToken: 'rt-2', subject: 'person-1' })
    expect(event.written['portal_session']?.options).toMatchObject({ maxAge: 30 * 24 * 60 * 60 })
  })

  it('answers nothing without a session', async () => {
    expect(await resolveAccessToken(makeEvent())).toBeUndefined()
  })
})

describe('refreshSession', () => {
  it('keeps the refresh token when the SSO did not rotate it', async () => {
    sso.respondWith(() => ({ body: { access_token: 'fresh', expires_in: 900 } }))
    const cookies = await signedIn('at', { refreshToken: 'rt' })

    expect(await refreshSession(makeEvent({ cookies }))).toBe('fresh')
    expect(await storedSession(cookies)).toMatchObject({ accessToken: 'fresh', refreshToken: 'rt' })
  })

  it('shares one exchange between concurrent requests of one session', async () => {
    sso.respondWith(() => ({ body: { access_token: 'fresh', refresh_token: 'rt-2', expires_in: 900 } }))
    const cookies = await signedIn('at', { refreshToken: 'rt' })

    const results = await Promise.all([refreshSession(makeEvent({ cookies })), refreshSession(makeEvent({ cookies }))])

    expect(results).toEqual(['fresh', 'fresh'])
    expect(sso.requests.filter((request) => request.url === '/connect/token')).toHaveLength(1)
  })

  it('ends a session that has no refresh token without calling the SSO', async () => {
    const cookies = await signedIn('at')

    expect(await refreshSession(makeEvent({ cookies }))).toBeNull()
    expect(sso.requests).toHaveLength(0)
    expect(await storedSession(cookies)).toBeNull()
  })
})

describe('refreshSession when the renewal cannot be completed', () => {
  it.each([400, 401])('ends the session when the SSO refuses the grant with %i', async (status) => {
    sso.respondWith(() => ({ status, body: { error: 'invalid_grant' } }))
    const cookies = await signedIn('at', { refreshToken: 'rt' })
    const event = makeEvent({ cookies })

    expect(await refreshSession(event)).toBeNull()
    expect(await storedSession(cookies)).toBeNull()
    expect(event.deleted).toEqual(expect.arrayContaining(['portal_session', 'authed']))
  })

  it.each([500, 502, 503])('answers 503 and keeps the session when the SSO fails with %i', async (status) => {
    sso.respondWith(() => ({ status, body: {} }))
    const cookies = await signedIn('at', { refreshToken: 'rt' })
    const event = makeEvent({ cookies })

    const error = await refusal(() => refreshSession(event))

    expect(error).toMatchObject({ statusCode: 503, data: { code: 'SIGN_ON_UNAVAILABLE' } })
    expect(await storedSession(cookies)).toMatchObject({ accessToken: 'at', refreshToken: 'rt' })
    expect(event.deleted).toEqual([])
  })

  it('answers 503 and keeps the session when the SSO cannot be reached', async () => {
    // Nothing listens on port 1, so the connection is refused.
    setRuntimeConfig({ ssoUrl: 'http://127.0.0.1:1' })
    const cookies = await signedIn('at', { refreshToken: 'rt' })

    const error = await refusal(() => refreshSession(makeEvent({ cookies })))

    expect(error).toMatchObject({ statusCode: 503 })
    expect(await storedSession(cookies)).toMatchObject({ refreshToken: 'rt' })
  })

  it('answers 503 and keeps the session when the SSO answers without an access token', async () => {
    sso.respondWith(() => ({ body: {} }))
    const cookies = await signedIn('at', { refreshToken: 'rt' })

    const error = await refusal(() => refreshSession(makeEvent({ cookies })))

    expect(error).toMatchObject({ statusCode: 503 })
    expect(await storedSession(cookies)).toMatchObject({ accessToken: 'at' })
  })
})

describe('refreshSession across replicas', () => {
  afterEach(() => {
    vi.useRealTimers()
  })

  it('releases the refresh lock once the exchange is done, so the next renewal can take it', async () => {
    sso.respondWith(() => ({ body: { access_token: 'fresh', refresh_token: 'rt-2', expires_in: 900 } }))
    const cookies = await signedIn('at', { refreshToken: 'rt' })

    await refreshSession(makeEvent({ cookies }))

    const holder = await useSessionStore().acquireRefreshLock(cookies['portal_session'] ?? '', 15)
    expect(holder).toBeTruthy()
  })

  it('releases the refresh lock when the exchange fails', async () => {
    sso.respondWith(() => ({ status: 500, body: {} }))
    const cookies = await signedIn('at', { refreshToken: 'rt' })

    await refusal(() => refreshSession(makeEvent({ cookies })))

    expect(await useSessionStore().acquireRefreshLock(cookies['portal_session'] ?? '', 15)).toBeTruthy()
  })

  it('uses the renewal another replica finished while it waited, and presents no token itself', async () => {
    vi.useFakeTimers({ toFake: ['setTimeout'] })
    const cookies = await signedIn('at', { refreshToken: 'rt', subject: 'person-1' })
    const id = cookies['portal_session'] ?? ''
    const store = useSessionStore()
    const otherReplica = await store.acquireRefreshLock(id, 15)

    const pending = refreshSession(makeEvent({ cookies }))
    await vi.advanceTimersByTimeAsync(250)
    await store.set(id, { accessToken: 'renewed-elsewhere', refreshToken: 'rt-2', subject: 'person-1' }, 3600)
    await store.releaseRefreshLock(id, otherReplica ?? '')
    await vi.advanceTimersByTimeAsync(250)

    expect(await pending).toBe('renewed-elsewhere')
    expect(sso.requests).toHaveLength(0)
  })

  it('answers 503 and keeps the session when the lock stays with another replica', async () => {
    vi.useFakeTimers({ toFake: ['setTimeout'] })
    const cookies = await signedIn('at', { refreshToken: 'rt' })
    await useSessionStore().acquireRefreshLock(cookies['portal_session'] ?? '', 600)

    const pending = refusal(() => refreshSession(makeEvent({ cookies })))
    await vi.advanceTimersByTimeAsync(20 * 250 + 250)

    expect(await pending).toMatchObject({ statusCode: 503 })
    expect(await storedSession(cookies)).toMatchObject({ accessToken: 'at' })
    expect(sso.requests).toHaveLength(0)
  })
})

describe('updateAccessToken', () => {
  it('replaces the access token and keeps the refresh token and owner', async () => {
    const cookies = await signedIn('at', { refreshToken: 'rt', subject: 'person-1' })

    await updateAccessToken(makeEvent({ cookies }), 'renewed')

    expect(await storedSession(cookies)).toMatchObject({ accessToken: 'renewed', refreshToken: 'rt', subject: 'person-1' })
  })

  it('does nothing without a session', async () => {
    const event = makeEvent()

    await updateAccessToken(event, 'renewed')

    expect(event.written).toEqual({})
  })
})

describe('endSession', () => {
  it('deletes the stored entry and clears the session, flag and legacy cookies', async () => {
    const cookies = await signedIn('at', { refreshToken: 'rt' })
    const event = makeEvent({ cookies })

    await endSession(event)

    expect(await storedSession(cookies)).toBeNull()
    expect(event.deleted).toEqual(expect.arrayContaining(['portal_session', 'authed', 'auth_token', 'refresh_token']))
  })
})
