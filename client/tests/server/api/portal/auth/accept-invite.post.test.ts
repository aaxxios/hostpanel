/**
 * Tests for `POST /api/portal/auth/accept-invite`: accepting a team invitation needs a signed-in
 * session, sends only the token, and turns a missing or expired session into the sign-in-required code.
 *
 * @module tests/server/api/portal/auth/accept-invite.post.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend } from '../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../helpers/bffHarness'
import type { StubServer } from '../../../../helpers/stubServer'
import { PortalErrorCode } from '~/constants/portalErrorCode'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/auth/accept-invite.post')
})

afterEach(async () => {
  await backend.close()
})

describe('POST /api/portal/auth/accept-invite', () => {
  it('sends only the token to the backend with the session bearer and the reader language', async () => {
    backend.respondWith(() => ({ body: { success: true } }))
    const event = makeEvent({ body: { token: 'inv-1', extra: 'dropped' }, cookies: { auth_token: 'jwt' }, headers: { 'x-language': 'ru' } })

    const result = await route.default(event)

    expect(result).toEqual({ success: true })
    expect(backend.requests[0]?.url).toBe('/api/auth/accept-invite')
    expect(backend.requests[0]?.headers['authorization']).toBe('Bearer jwt')
    expect(backend.requests[0]?.headers['x-language']).toBe('ru')
    expect(JSON.parse(backend.requests[0]?.body ?? '{}')).toEqual({ token: 'inv-1' })
  })

  it('sends an empty token when the body carries no string token', async () => {
    await route.default(makeEvent({ body: { token: 5 }, cookies: { auth_token: 'jwt' } }))

    expect(JSON.parse(backend.requests[0]?.body ?? '{}')).toEqual({ token: '' })
  })

  it('refuses with the sign-in-required code, without calling the backend, when there is no session', async () => {
    const error = await refusal(() => route.default(makeEvent({ body: { token: 'inv-1' } })))

    expect(error).toMatchObject({ statusCode: 403, data: { code: PortalErrorCode.InviteSignInRequired } })
    expect(backend.requests).toHaveLength(0)
  })

  it('maps a backend 401 to the sign-in-required code', async () => {
    backend.respondWith(() => ({ status: 401, body: { type: 'UNAUTHORIZED', detail: 'Expired.' } }))

    const error = await refusal(() => route.default(makeEvent({ body: { token: 'inv-1' }, cookies: { auth_token: 'jwt' } })))

    expect(error).toMatchObject({ statusCode: 403, data: { code: PortalErrorCode.InviteSignInRequired } })
  })

  it('passes any other backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 410, body: { type: 'INVITE_EXPIRED', detail: 'The invitation expired.' } }))

    const error = await refusal(() => route.default(makeEvent({ body: { token: 'inv-1' }, cookies: { auth_token: 'jwt' } })))

    expect(error).toMatchObject({ statusCode: 410, statusMessage: 'The invitation expired.', data: { code: 'INVITE_EXPIRED' } })
  })
})
