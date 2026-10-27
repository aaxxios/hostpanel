/**
 * Tests for the BFF transport in `server/utils/api.ts`, run against a real HTTP stub that plays
 * the C# API (and the SSO token endpoint for the refresh), over the real in-process session store.
 *
 * The language proof for the second hop: the `X-Language` the browser sent must reach the backend
 * on every call, the retry after a token refresh included. The error half: a refusal is
 * forwarded as the backend's own `detail` (never an invented sentence) with its code, title and
 * field errors in `data`, and an answer-less backend leaks no upstream address.
 *
 * @module tests/server/utils/api.test
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import type { H3Event } from 'h3'
import { ofetch } from 'ofetch'
import { startStubServer } from '../../helpers/stubServer'
import type { StubReply, StubRequest, StubServer } from '../../helpers/stubServer'

/** A fake H3 event: the incoming headers, cookies, and the cookies the code under test writes. */
interface FakeEvent {
  /** Incoming request headers, lower-cased. */
  headers: Record<string, string>
  /** Incoming cookies. */
  cookies: Record<string, string>
  /** Cookies written to the response, by name. */
  written: Record<string, string>
  /** Cookies deleted from the response. */
  deleted: string[]
}

/** The stub playing the backend and the SSO. */
let stub: StubServer

/** Id of the signed-in session every event carries by default; it holds the access token `old`. */
const SIGNED_IN = 'session-signed-in'

/** Id of a session that also holds a refresh token. */
const REFRESHABLE = 'session-refreshable'

/**
 * Builds a fake event.
 *
 * @param headers - Incoming headers (lower-cased names).
 * @param cookies - Incoming cookies.
 * @returns The event, typed as the H3 event the code under test expects.
 */
const makeEvent = (headers: Record<string, string> = {}, cookies: Record<string, string> = { portal_session: SIGNED_IN }): H3Event & FakeEvent => {
  return { headers, cookies, written: {}, deleted: [] } as unknown as H3Event & FakeEvent
}

vi.stubGlobal('getHeader', (event: FakeEvent, name: string) => event.headers[name.toLowerCase()])
vi.stubGlobal('getCookie', (event: FakeEvent, name: string) => event.cookies[name])
vi.stubGlobal('setCookie', (event: FakeEvent, name: string, value: string) => {
  event.written[name] = value
})
vi.stubGlobal('deleteCookie', (event: FakeEvent, name: string) => {
  event.deleted.push(name)
})
vi.stubGlobal('createError', (options: Record<string, unknown>) => Object.assign(new Error(String(options.statusMessage ?? '')), options))
vi.stubGlobal('useRuntimeConfig', () => ({ apiUrl: stub.baseUrl, ssoUrl: stub.baseUrl, ssoClientId: 'client', ssoClientSecret: 'secret' }))

const { backendLanguage, internalApiCall } = await import('~/server/utils/api')
// No `sessionRedis` in the runtime config: the real store is the in-process one.
const { useSessionStore } = await import('~/server/utils/session-store')

beforeEach(async () => {
  stub = await startStubServer()
  vi.stubGlobal('$fetch', ofetch)
  await useSessionStore().set(SIGNED_IN, { accessToken: 'old' }, 3600)
  await useSessionStore().set(REFRESHABLE, { accessToken: 'old', refreshToken: 'r' }, 3600)
})

afterEach(async () => {
  await stub.close()
})

/**
 * Requests that reached the backend path (the SSO token endpoint excluded).
 *
 * @returns The backend requests, in order.
 */
const backendRequests = (): StubRequest[] => stub.requests.filter((request) => request.url.startsWith('/api/'))

describe('backendLanguage', () => {
  it.each(['en', 'ru', 'hy'])('passes the supported language %s through', (language) => {
    expect(backendLanguage(makeEvent({ 'x-language': language }))).toBe(language)
  })

  it('falls back to the default when the header is missing, unsupported, or the retired x-locale', () => {
    expect(backendLanguage(makeEvent())).toBe('en')
    expect(backendLanguage(makeEvent({ 'x-language': 'fr' }))).toBe('en')
    expect(backendLanguage(makeEvent({ 'x-locale': 'ru' }))).toBe('en')
  })
})

describe('internalApiCall sliding session', () => {
  it('stores a renewed token from the backend in the server-side session, not in a cookie', async () => {
    stub.respondWith(() => ({ body: { ok: true }, headers: { 'X-Renewed-Access-Token': 'renewed' } }))
    const event = makeEvent()

    await internalApiCall(event, '/me/services')

    expect(await useSessionStore().get(SIGNED_IN)).toMatchObject({ accessToken: 'renewed' })
    expect(event.written['portal_session']).toBe(SIGNED_IN)
    expect(event.written['auth_token']).toBeUndefined()
  })

  it('leaves the session alone when the backend offers no renewal', async () => {
    const event = makeEvent()

    await internalApiCall(event, '/me/services')

    expect(await useSessionStore().get(SIGNED_IN)).toMatchObject({ accessToken: 'old' })
    expect(event.written).toEqual({})
  })
})

describe('internalApiCall language', () => {
  it('sends the browser X-Language and the bearer token to the backend', async () => {
    await internalApiCall(makeEvent({ 'x-language': 'hy' }), '/clients/me')

    expect(backendRequests()[0]?.url).toBe('/api/clients/me')
    expect(backendRequests()[0]?.headers['x-language']).toBe('hy')
    expect(backendRequests()[0]?.headers['authorization']).toBe('Bearer old')
  })

  it('sends the CSRF header on an anonymous state-changing call, which the backend refuses without it', async () => {
    await internalApiCall(makeEvent(), '/domains/check', { method: 'POST', body: { domain: 'a.am' }, anonymous: true })

    expect(backendRequests()[0]?.headers['x-requested-with']).toBe('XMLHttpRequest')
    expect(backendRequests()[0]?.headers['authorization']).toBeUndefined()
  })

  it('sends the default language when the browser sent none or an unsupported one', async () => {
    await internalApiCall(makeEvent(), '/clients/me')
    await internalApiCall(makeEvent({ 'x-language': 'zz' }), '/clients/me')

    expect(backendRequests().map((request) => request.headers['x-language'])).toEqual(['en', 'en'])
  })

  it('keeps the language on the retry after a token refresh', async () => {
    stub.respondWith((request) => {
      if (request.url === '/connect/token') {
        return { body: { access_token: 'fresh', expires_in: 900 } }
      }
      return request.headers['authorization'] === 'Bearer fresh' ? { body: { ok: true } } : { status: 401, body: {} }
    })
    const event = makeEvent({ 'x-language': 'ru' }, { portal_session: REFRESHABLE })

    const result = await internalApiCall(event, '/me/services')

    expect(result).toEqual({ ok: true })
    expect(backendRequests().map((request) => [request.headers['authorization'], request.headers['x-language']])).toEqual([
      ['Bearer old', 'ru'],
      ['Bearer fresh', 'ru'],
    ])
    expect(await useSessionStore().get(REFRESHABLE)).toMatchObject({ accessToken: 'fresh', refreshToken: 'r' })
  })

  it('ends the session and answers 401 when the refresh is refused', async () => {
    stub.respondWith((request) => request.url === '/connect/token' ? { status: 400, body: { error: 'invalid_grant' } } : { status: 401, body: {} })
    const event = makeEvent({}, { portal_session: REFRESHABLE })

    await expect(internalApiCall(event, '/me/services')).rejects.toMatchObject({ statusCode: 401 })

    expect(await useSessionStore().get(REFRESHABLE)).toBeNull()
    expect(event.deleted).toEqual(expect.arrayContaining(['portal_session', 'authed']))
  })

  it('ends a session without a refresh token on a 401, since nothing can renew it', async () => {
    stub.respondWith(() => ({ status: 401, body: {} }))
    const event = makeEvent()

    await expect(internalApiCall(event, '/me/services')).rejects.toMatchObject({ statusCode: 401 })

    expect(stub.requests.some((request) => request.url === '/connect/token')).toBe(false)
    expect(await useSessionStore().get(SIGNED_IN)).toBeNull()
  })

  it('does not refresh, and sends no bearer, for an anonymous call', async () => {
    stub.respondWith(() => ({ status: 401, body: { type: 'INVALID_CREDENTIALS', detail: 'Wrong.' } }))
    const event = makeEvent({ 'x-language': 'ru' }, { portal_session: REFRESHABLE })

    await expect(internalApiCall(event, '/auth/login', { method: 'POST', body: { email: 'a' }, anonymous: true })).rejects.toMatchObject({ statusCode: 401 })

    expect(stub.requests).toHaveLength(1)
    expect(stub.requests[0]?.headers['authorization']).toBeUndefined()
    expect(stub.requests[0]?.headers['x-language']).toBe('ru')
    expect(JSON.parse(stub.requests[0]?.body ?? '{}')).toEqual({ email: 'a' })
  })
})

describe('internalApiCall refusals', () => {
  /**
   * Scripts a refusal and returns what the route would rethrow.
   *
   * @param reply - The backend answer.
   * @returns The thrown error as a plain record.
   */
  const refusedWith = async (reply: StubReply): Promise<Record<string, unknown>> => {
    stub.respondWith(() => reply)
    try {
      await internalApiCall(makeEvent(), '/clients/me')
    } catch (error) {
      return error as Record<string, unknown>
    }
    throw new Error('the call was expected to be refused')
  }

  it('forwards the backend detail as the sentence and its code, title and field errors as data', async () => {
    const error = await refusedWith({
      status: 400,
      body: { type: 'VALIDATION_ERROR', title: 'Validation failed', detail: 'Проверьте поля.', errors: { email: ['Неверный email.'] } },
    })

    expect(error).toMatchObject({
      statusCode: 400,
      statusMessage: 'Проверьте поля.',
      data: { code: 'VALIDATION_ERROR', title: 'Validation failed', errors: { email: ['Неверный email.'] } },
    })
  })

  it('falls back to the title when the backend sent no detail', async () => {
    const error = await refusedWith({ status: 404, body: { type: 'NOT_FOUND', title: 'Not found' } })

    expect(error).toMatchObject({ statusCode: 404, statusMessage: 'Not found', data: { code: 'NOT_FOUND' } })
  })

  it('invents no sentence and leaks no address when the backend sent no body', async () => {
    const error = await refusedWith({ status: 500, body: '' })

    expect(error.statusCode).toBe(500)
    expect(error.statusMessage).toBeUndefined()
    expect(error.data).toEqual({ code: null, title: null, errors: null })
    expect(JSON.stringify(error)).not.toContain(stub.baseUrl)
  })

  it('answers 502 with no sentence when the backend cannot be reached', async () => {
    vi.stubGlobal('useRuntimeConfig', () => ({ apiUrl: 'http://127.0.0.1:1', ssoUrl: stub.baseUrl }))

    await expect(internalApiCall(makeEvent(), '/clients/me')).rejects.toMatchObject({ statusCode: 502, data: { code: null } })
  })
})
