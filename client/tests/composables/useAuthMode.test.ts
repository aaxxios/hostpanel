/**
 * Tests for `useAuthMode`: the mode comes from the public runtime config, defaults to SSO, and the
 * sign-in destination follows from it.
 *
 * @module tests/composables/useAuthMode.test
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useAuthMode } from '~/composables/useAuthMode'

/**
 * Stubs the runtime config and the locale-aware path helper.
 *
 * @param authMode The `authMode` value the deployment configured.
 */
const stubDeployment = (authMode: string | undefined): void => {
  vi.stubGlobal('useRuntimeConfig', () => ({ public: { authMode } }))
  vi.stubGlobal('useLocalePath', () => (path: string) => `/ru${path}`)
}

describe('useAuthMode', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('defaults to SSO and sends the visitor to the authorize route', () => {
    stubDeployment(undefined)

    const { mode, isSso, signInHref } = useAuthMode()

    expect(mode.value).toBe('sso')
    expect(isSso.value).toBe(true)
    expect(signInHref.value).toBe('/api/portal/auth/sso/authorize')
  })

  it('sends a local deployment to the localised login page', () => {
    stubDeployment('local')

    const { isSso, signInHref } = useAuthMode()

    expect(isSso.value).toBe(false)
    expect(signInHref.value).toBe('/ru/client/login')
  })
})
