/**
 * Tests for `composables/apis/useAuthApi.ts`.
 *
 * Each endpoint function is called for real through `apiFetch` against a stub server, and the
 * test asserts on the path, verb, query and body that arrived and on how the reply was typed.
 *
 * @module tests/composables/apis/useAuthApi.test
 */

import { afterEach, describe, expect, it } from 'vitest'
import { expectLastRequest, startTransport } from '../../helpers/stubTransport'
import type { ExpectedRequest } from '../../helpers/stubTransport'
import type { StubServer } from '../../helpers/stubServer'
import { useAuthApi } from '~/composables/apis/useAuthApi'

/** The stub started by the current test. */
let stub: StubServer | undefined

afterEach(async () => {
  await stub?.close()
  stub = undefined
})

/** The api surface under test. */
const api = useAuthApi()

/** Every endpoint function with the request it must send. */
const rows: ExpectedRequest[] = [
  { name: 'login posts the credentials', call: () => api.login('a@b.c', 'pw'), method: 'POST', url: '/api/portal/auth/login', body: { email: 'a@b.c', password: 'pw' } },
  { name: 'loginWithTwoFactor posts the pending token and code', call: () => api.loginWithTwoFactor('pend', '123456'), method: 'POST', url: '/api/portal/auth/2fa-login', body: { pendingToken: 'pend', code: '123456' } },
  { name: 'register posts the payload', call: () => api.register({ firstname: 'A', lastname: 'B', email: 'a@b.c', password: 'pw' }), method: 'POST', url: '/api/portal/auth/register', body: { firstname: 'A', lastname: 'B', email: 'a@b.c', password: 'pw' } },
  { name: 'logout posts without a body', call: () => api.logout(), method: 'POST', url: '/api/portal/auth/logout' },
  { name: 'ssoLogout posts without a body', call: () => api.ssoLogout(), method: 'POST', url: '/api/portal/auth/sso/logout' },
  { name: 'requestPasswordReset posts the email', call: () => api.requestPasswordReset('a@b.c'), method: 'POST', url: '/api/portal/auth/forgot-password', body: { email: 'a@b.c' } },
  { name: 'resetPassword posts email, token and the new password', call: () => api.resetPassword('a@b.c', 'tok', 'newpw'), method: 'POST', url: '/api/portal/auth/reset-password', body: { email: 'a@b.c', token: 'tok', newPassword: 'newpw' } },
  { name: 'confirmEmail posts email and token', call: () => api.confirmEmail('a@b.c', 'tok'), method: 'POST', url: '/api/portal/auth/confirm-email', body: { email: 'a@b.c', token: 'tok' } },
  { name: 'acceptInvite posts the token', call: () => api.acceptInvite('tok'), method: 'POST', url: '/api/portal/auth/accept-invite', body: { token: 'tok' } },
  { name: 'setupTwoFactor posts without a body', call: () => api.setupTwoFactor(), method: 'POST', url: '/api/portal/auth/2fa-setup' },
  { name: 'enableTwoFactor posts the code', call: () => api.enableTwoFactor('123456'), method: 'POST', url: '/api/portal/auth/2fa-enable', body: { code: '123456' } },
  { name: 'disableTwoFactor posts the code', call: () => api.disableTwoFactor('123456'), method: 'POST', url: '/api/portal/auth/2fa-disable', body: { code: '123456' } },
]

describe('useAuthApi', () => {
  it.each(rows)('$name', async (row) => {
    stub = await startTransport()

    await row.call()

    expectLastRequest(stub, row)
  })

  it('returns the second-factor challenge untouched so the store can tell it from an account', async () => {
    stub = await startTransport(() => ({ body: { twoFactorRequired: true, pendingToken: 'pend' } }))

    expect(await api.login('a@b.c', 'pw')).toEqual({ twoFactorRequired: true, pendingToken: 'pend' })
  })
})
