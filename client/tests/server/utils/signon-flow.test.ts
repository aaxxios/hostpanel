/**
 * Tests for `server/utils/signon-flow.ts`: the shared start of the sign-on flow binds a fresh PKCE
 * pair and `state` to the browser, builds the authorize query, and fails closed before any cookie.
 *
 * @module tests/server/utils/signon-flow.test
 */

import { createHash } from 'node:crypto'
import { beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, setRuntimeConfig } from '../../helpers/bffHarness'
import { safeReturnPath, startSignonFlow } from '~/server/utils/signon-flow'

beforeEach(() => {
  setRuntimeConfig({
    ssoClientId: 'client',
    ssoCallbackUrl: 'https://portal.test/api/portal/auth/sso/callback',
    public: { ssoPublicUrl: 'https://sso.test' },
  })
})

describe('startSignonFlow', () => {
  it('returns the provider address and an authorize query whose challenge matches the verifier cookie', () => {
    const event = makeEvent()

    const { ssoPublicUrl, params } = startSignonFlow(event, null)

    expect(ssoPublicUrl).toBe('https://sso.test')
    expect(params.get('client_id')).toBe('client')
    expect(params.get('response_type')).toBe('code')
    expect(params.get('redirect_uri')).toBe('https://portal.test/api/portal/auth/sso/callback')
    expect(params.get('scope')).toBe('openid profile email offline_access')
    expect(params.get('code_challenge_method')).toBe('S256')
    expect(params.get('state')).toBe(event.written['oauth_state']?.value)
    const verifier = event.written['pkce_verifier']?.value ?? ''
    expect(params.get('code_challenge')).toBe(createHash('sha256').update(verifier).digest('base64url'))
  })

  it('writes the flow cookies httpOnly, Secure, SameSite=Lax for five minutes', () => {
    const event = makeEvent()

    startSignonFlow(event, '/client/invoices')

    for (const name of ['pkce_verifier', 'oauth_state', 'post_login_redirect']) {
      expect(event.written[name]?.options, name).toMatchObject({ httpOnly: true, secure: true, sameSite: 'lax', path: '/', maxAge: 300 })
    }
    expect(event.written['post_login_redirect']?.value).toBe('/client/invoices')
  })

  it('writes no return-path cookie when none is given', () => {
    const event = makeEvent()

    startSignonFlow(event, null)

    expect(Object.keys(event.written).sort()).toEqual(['oauth_state', 'pkce_verifier'])
  })

  it('uses a new verifier and state on every start', () => {
    const first = makeEvent()
    const second = makeEvent()

    startSignonFlow(first, null)
    startSignonFlow(second, null)

    expect(first.written['pkce_verifier']?.value).not.toBe(second.written['pkce_verifier']?.value)
    expect(first.written['oauth_state']?.value).not.toBe(second.written['oauth_state']?.value)
  })

  it.each([
    [{ ssoCallbackUrl: 'https://portal.test/cb', public: { ssoPublicUrl: '' } }],
    [{ ssoCallbackUrl: '', public: { ssoPublicUrl: 'https://sso.test' } }],
  ])('refuses with a coded 503 and writes no cookie when an address is empty (%j)', async (config) => {
    setRuntimeConfig(config)
    const event = makeEvent()

    const error = await refusal(() => startSignonFlow(event, '/client'))

    expect(error).toMatchObject({ statusCode: 503, data: { code: 'SSO_NOT_CONFIGURED' } })
    expect(event.written).toEqual({})
  })
})

describe('safeReturnPath', () => {
  it('keeps a path inside the app', () => {
    expect(safeReturnPath('/client/domains')).toBe('/client/domains')
  })

  it.each([undefined, '', 'https://elsewhere.test/x', '//elsewhere.test', '/\\elsewhere.test', '/api/portal/x'])(
    'refuses %p',
    (candidate) => {
      expect(safeReturnPath(candidate)).toBeNull()
    },
  )
})
