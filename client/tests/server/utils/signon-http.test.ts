/**
 * Tests for `server/utils/signon-http.ts`, against a real HTTP stub playing the sign-on service:
 * the call carries the bearer and the body, and every failure reaches the caller as a typed error
 * (a status and `data.code`) that never contains the internal address.
 *
 * @module tests/server/utils/signon-http.test
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { refusal, startBackend } from '../../helpers/bffHarness'
import type { StubServer } from '../../helpers/stubServer'
import { SIGN_ON_TIMEOUT_MS, signOnFetch } from '~/server/utils/signon-http'
import { HttpMethod } from '~/types/httpmethod'

/** The stub playing the sign-on service. */
let signOn: StubServer

beforeEach(async () => {
  signOn = await startBackend()
})

afterEach(async () => {
  await signOn.close()
})

describe('signOnFetch', () => {
  it('sends the bearer, the method and the body, and returns the parsed reply', async () => {
    signOn.respondWith(() => ({ body: { email: 'a@b.test' } }))

    const result = await signOnFetch<{ email: string }>('/api/account/profile', {
      method: HttpMethod.Post,
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      body: 'a=1',
      accessToken: 'jwt',
    })

    expect(result).toEqual({ email: 'a@b.test' })
    expect(signOn.requests[0]).toMatchObject({ method: 'POST', url: '/api/account/profile', body: 'a=1' })
    expect(signOn.requests[0]?.headers['authorization']).toBe('Bearer jwt')
  })

  it('sends no bearer without an access token', async () => {
    await signOnFetch('/connect/token')

    expect(signOn.requests[0]?.headers['authorization']).toBeUndefined()
  })

  it.each([400, 401, 403, 404])('keeps the status of a %i refusal and carries only a code', async (status) => {
    signOn.respondWith(() => ({ status, body: { error: 'invalid_grant' } }))

    const error = await refusal(() => signOnFetch('/connect/token'))

    expect(error).toMatchObject({ statusCode: status, data: { code: 'SIGN_ON_REFUSED' } })
    expect(error['statusMessage']).toBeUndefined()
    expect(String(error['message'] ?? '')).not.toContain(signOn.baseUrl)
  })

  it.each([500, 502, 503])('answers 502 for a %i from the service', async (status) => {
    signOn.respondWith(() => ({ status, body: {} }))

    const error = await refusal(() => signOnFetch('/api/account/profile', { accessToken: 'jwt' }))

    expect(error).toMatchObject({ statusCode: 502, data: { code: 'SIGN_ON_UNAVAILABLE' } })
    expect(String(error['message'] ?? '')).not.toContain(signOn.baseUrl)
  })

  it('answers 502 without leaking the address when the service cannot be reached', async () => {
    await signOn.close()

    const error = await refusal(() => signOnFetch('/api/account/profile'))
    signOn = await startBackend()

    expect(error).toMatchObject({ statusCode: 502, data: { code: 'SIGN_ON_UNAVAILABLE' } })
    expect(String(error['message'] ?? '')).not.toContain('127.0.0.1')
  })

  it('answers 504 when the service does not answer in time, and asks for the timeout', async () => {
    const fetcher = vi.fn().mockRejectedValue(Object.assign(new Error('timed out'), { name: 'TimeoutError' }))
    vi.stubGlobal('$fetch', fetcher)

    const error = await refusal(() => signOnFetch('/api/account/profile'))

    expect(error).toMatchObject({ statusCode: 504, data: { code: 'SIGN_ON_TIMEOUT' } })
    expect(fetcher).toHaveBeenCalledWith(expect.any(String), expect.objectContaining({ timeout: SIGN_ON_TIMEOUT_MS, retry: 0 }))
  })

  it('treats an aborted request wrapped by the fetch layer as a timeout', async () => {
    vi.stubGlobal('$fetch', vi.fn().mockRejectedValue(Object.assign(new Error('failed'), { cause: { name: 'AbortError' } })))

    const error = await refusal(() => signOnFetch('/api/account/profile'))

    expect(error).toMatchObject({ statusCode: 504, data: { code: 'SIGN_ON_TIMEOUT' } })
  })
})
