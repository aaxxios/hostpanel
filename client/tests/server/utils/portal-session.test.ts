/**
 * Tests for `server/utils/portal-session.ts`, over the real in-process session store and a real
 * HTTP stub playing the SSO token endpoint: a sign-in keeps the tokens on the server and gives the
 * browser one small opaque cookie, an expiring access token is renewed with the refresh token
 * before it is used, concurrent renewals share one exchange, and signing out deletes the entry.
 *
 * @module tests/server/utils/portal-session.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, signedIn, startBackend, storedSession } from '../../helpers/bffHarness'
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

/** The current time in seconds. */
const now = (): number => Math.floor(Date.now() / 1000)

describe('startSession', () => {
  it('stores the tokens and their owner under a random id and sets only that id as an httpOnly cookie', async () => {
    const accessToken = jwt({ sub: 'person-1', exp: now() + 900 })
    const event = makeEvent()

    await startSession(event, { accessToken, refreshToken: 'r'.repeat(5000), expiresIn: 900 })

    const cookie = event.written['portal_session']
    expect(cookie?.value).toMatch(/^[A-Za-z0-9_-]{43}$/)
    expect(cookie?.options).toMatchObject({ httpOnly: true, sameSite: 'lax', path: '/', maxAge: 30 * 24 * 60 * 60 })
    expect(event.written['authed']).toMatchObject({ value: '1', options: { httpOnly: false } })
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

  it('ends the session when the SSO refuses the refresh token', async () => {
    sso.respondWith(() => ({ status: 400, body: { error: 'invalid_grant' } }))
    const cookies = await signedIn('at', { refreshToken: 'rt' })
    const event = makeEvent({ cookies })

    expect(await refreshSession(event)).toBeNull()
    expect(await storedSession(cookies)).toBeNull()
    expect(event.deleted).toEqual(expect.arrayContaining(['portal_session', 'authed']))
  })

  it('ends a session that has no refresh token without calling the SSO', async () => {
    const cookies = await signedIn('at')

    expect(await refreshSession(makeEvent({ cookies }))).toBeNull()
    expect(sso.requests).toHaveLength(0)
    expect(await storedSession(cookies)).toBeNull()
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
