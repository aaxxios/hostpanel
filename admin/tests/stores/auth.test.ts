/**
 * Tests for the auth store's refusal wording: the API's own sentence wins, the only client
 * sentences are the shared no-answer line and the sign-in loop line, and there is no
 * status-to-sentence table.
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { useAuthStore } from '@/stores/auth'
import { callOf, json, problem, stubFetch, stubOffline } from '../helpers/fetch'
import { NO_ANSWER } from '../helpers/storeHooks'

describe('auth store sentences', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('shows the sentence the API wrote for a rejected sign-in', async () => {
    stubFetch(problem('Wrong password.', 401))
    const store = useAuthStore()

    expect(await store.signInLocal('a@b.c', 'x')).toBe('failed')
    expect(store.error).toBe('Wrong password.')
  })

  it('shows the no-answer sentence for a refusal without a body', async () => {
    stubFetch(new Response(null, { status: 429 }))
    const store = useAuthStore()

    expect(await store.signInLocal('a@b.c', 'x')).toBe('failed')
    expect(store.error).toBe(NO_ANSWER)
  })

  it('shows the no-answer sentence when the request never arrived', async () => {
    stubOffline()
    const store = useAuthStore()

    expect(await store.signInLocal('a@b.c', 'x')).toBe('failed')
    expect(store.error).toBe(NO_ANSWER)
  })

  it('shows the sign-in loop sentence when no session is returned', async () => {
    stubFetch(json({}))
    const store = useAuthStore()

    expect(await store.signInLocal('a@b.c', 'x')).toBe('failed')
    expect(store.error).toBe('Signed in, but the session was not accepted. Try again.')
  })

  it('lets the API refuse a second-factor step that has no pending token', async () => {
    const fetchMock = stubFetch(problem('Sign-in attempt not found.', 401))
    const store = useAuthStore()

    expect(await store.submitTwoFactor('123456')).toBe('failed')
    // The request goes out with an empty token, and the store shows the API's own detail.
    expect(fetchMock).toHaveBeenCalledTimes(1)
    const init = fetchMock.mock.calls[0]?.[1] as RequestInit
    expect(JSON.parse(String(init.body))).toMatchObject({ pendingToken: '', code: '123456' })
    expect(store.error).toBe('Sign-in attempt not found.')
  })
})

describe('authStore.verifyEmail', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('returns true, posts the address and token and resets the local session state', async () => {
    const fetchMock = stubFetch(new Response(null, { status: 200 }))
    const store = useAuthStore()
    store.emailVerified = false

    expect(await store.verifyEmail('t k', 'a+b@c.d')).toBe(true)
    expect(fetchMock.mock.calls[0]![0]).toBe('/api/auth/confirm-email')
    expect(callOf(fetchMock).method).toBe('POST')
    expect(callOf(fetchMock).body).toEqual({ email: 'a+b@c.d', token: 't k' })
    expect(store.emailVerified).toBeNull()
  })

  it('returns false with the API sentence for an expired link', async () => {
    stubFetch(problem('The link has expired.'))
    const store = useAuthStore()

    expect(await store.verifyEmail('t', 'a@b.c')).toBe(false)
    expect(store.error).toBe('The link has expired.')
  })

  it('returns false with the fallback offline', async () => {
    stubOffline()
    const store = useAuthStore()

    expect(await store.verifyEmail('t', 'a@b.c')).toBe(false)
    expect(store.error).toBe('Could not reach the server. Check your connection and try again.')
  })
})

describe('authStore.resendVerification', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('posts to resend-verification with no body and returns true on 204', async () => {
    const fetchMock = stubFetch(new Response(null, { status: 204 }))

    expect(await useAuthStore().resendVerification()).toBe(true)
    expect(fetchMock.mock.calls[0]![0]).toBe('/api/auth/resend-verification')
    expect(callOf(fetchMock).method).toBe('POST')
    expect(callOf(fetchMock).init.body).toBeUndefined()
  })

  it('returns false with the API sentence when refused', async () => {
    stubFetch(problem('Too many requests.', 429))
    const store = useAuthStore()

    expect(await store.resendVerification()).toBe(false)
    expect(store.error).toBe('Too many requests.')
  })
})

describe('auth store session probe', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('treats a 401 from /auth/me as signed out, with no error and no console output', async () => {
    stubFetch(problem('Sign in.', 401))
    const logged = vi.spyOn(console, 'error').mockImplementation(() => undefined)
    const store = useAuthStore()

    await store.fetchMe()

    expect(store.isAuthenticated).toBe(false)
    expect(store.error).toBeNull()
    expect(logged).not.toHaveBeenCalled()
    logged.mockRestore()
  })

  it('loads the user when the session exists', async () => {
    stubFetch(json({ email: 'a@b.c', roles: ['Admin'], emailVerified: true }))
    const store = useAuthStore()

    await store.fetchMe()

    expect(store.user).toEqual({ email: 'a@b.c', roles: ['Admin'] })
    expect(store.emailVerified).toBe(true)
  })
})
