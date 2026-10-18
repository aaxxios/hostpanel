/**
 * Tests for the auth API composable's sign-out and SSO sign-in helpers: the URL it builds and the
 * end-session answer it reads, through the real `useApi` transport with `fetch` stubbed.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useAuthApi } from '@/composables/apis/useAuthApi'
import { LogoutScope } from '@/types/auth'
import { callOf, json, problem, stubFetch } from '../../helpers/fetch'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('useAuthApi', () => {
  it('verifyEmail posts the address and token to confirm-email', async () => {
    const mock = stubFetch(new Response(null, { status: 200 }))

    await useAuthApi().verifyEmail('tok', 'a@b.c')

    expect(callOf(mock).url).toBe('/api/auth/confirm-email')
    expect(callOf(mock).method).toBe('POST')
    expect(callOf(mock).body).toEqual({ email: 'a@b.c', token: 'tok' })
  })

  it('resendVerification posts to resend-verification without a body', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))

    await useAuthApi().resendVerification()

    expect(callOf(mock).url).toBe('/api/auth/resend-verification')
    expect(callOf(mock).method).toBe('POST')
  })

  it('ssoLoginUrl carries the page to return to', () => {
    expect(useAuthApi().ssoLoginUrl('/clients?page=2')).toBe('/api/auth/login?returnTo=%2Fclients%3Fpage%3D2')
  })

  it('endSessionUrl posts to logout and returns the SSO address', async () => {
    const mock = stubFetch(json({ endSessionUrl: 'https://sso.example/end' }))

    expect(await useAuthApi().endSessionUrl()).toBe('https://sso.example/end')
    expect(callOf(mock).url).toBe('/api/auth/logout')
    expect(callOf(mock).method).toBe('POST')
  })

  it('endSessionUrl asks for every product with scope all', async () => {
    const mock = stubFetch(json({}))

    expect(await useAuthApi().endSessionUrl(LogoutScope.All)).toBeNull()
    expect(callOf(mock).url).toBe('/api/auth/logout?scope=all')
  })

  it('endSessionUrl answers null when the API refuses', async () => {
    stubFetch(problem('Nope', 500))

    expect(await useAuthApi().endSessionUrl()).toBeNull()
  })
})
