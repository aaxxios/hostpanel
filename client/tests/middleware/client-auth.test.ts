/**
 * Tests for `middleware/client-auth.ts`: who may open which client page, in local and SSO mode.
 *
 * @module tests/middleware/client-auth.test
 */

import { beforeEach, describe, expect, it, vi } from 'vitest'
import { AuthMode } from '~/types/authmode'

/** The `authed` cookie the middleware reads. */
const authed = { value: null as string | null }

/** The runtime config public half. */
const publicConfig: Record<string, unknown> = {}

/** The recorded navigation. */
const navigateToMock = vi.fn()

/** The route a navigation targets. */
interface Target {
  /** Path without query. */
  path: string
  /** Path with query. */
  fullPath: string
}

/** The middleware under test. */
type Middleware = (to: Target) => unknown

/**
 * Loads the middleware with the Nuxt globals stubbed.
 *
 * @returns The route handler.
 */
const loadMiddleware = async (): Promise<Middleware> => {
  vi.stubGlobal('defineNuxtRouteMiddleware', (handler: Middleware) => handler)
  vi.stubGlobal('useCookie', () => authed)
  vi.stubGlobal('useRuntimeConfig', () => ({ public: publicConfig }))
  vi.stubGlobal('navigateTo', navigateToMock)
  return (await import('~/middleware/client-auth')).default as unknown as Middleware
}

beforeEach(() => {
  authed.value = null
  for (const key of Object.keys(publicConfig)) {
    Reflect.deleteProperty(publicConfig, key)
  }
  publicConfig.authMode = AuthMode.Local
  navigateToMock.mockReset()
  vi.resetModules()
})

describe('client-auth middleware', () => {
  it('sends an anonymous visitor of a private page to sign-in, carrying the page', async () => {
    const run = await loadMiddleware()

    run({ path: '/client/invoices', fullPath: '/client/invoices?page=2' })

    expect(navigateToMock).toHaveBeenCalledWith('/client/login?redirect=%2Fclient%2Finvoices%3Fpage%3D2')
  })

  it('lets an anonymous visitor open the sign-in page', async () => {
    const run = await loadMiddleware()

    run({ path: '/client/login', fullPath: '/client/login' })

    expect(navigateToMock).not.toHaveBeenCalled()
  })

  it('sends an anonymous visitor straight to the SSO authorize endpoint in SSO mode', async () => {
    publicConfig.authMode = AuthMode.Sso
    publicConfig.baseUrl = 'https://host.example'
    const run = await loadMiddleware()

    run({ path: '/client/dashboard', fullPath: '/client/dashboard' })

    expect(navigateToMock).toHaveBeenCalledWith(
      'https://host.example/api/portal/auth/sso/authorize',
      { external: true, replace: true },
    )
  })

  it('does not loop when the SSO authorize path itself is requested', async () => {
    publicConfig.authMode = AuthMode.Sso
    const run = await loadMiddleware()

    run({ path: '/api/portal/auth/sso/authorize', fullPath: '/api/portal/auth/sso/authorize' })

    expect(navigateToMock).not.toHaveBeenCalled()
  })

  it('sends a signed-in customer away from the sign-in page to the dashboard', async () => {
    authed.value = 'yes'
    const run = await loadMiddleware()

    run({ path: '/client/login', fullPath: '/client/login' })

    expect(navigateToMock).toHaveBeenCalledWith('/client/dashboard')
  })

  it('lets a signed-in customer open a private page', async () => {
    authed.value = 'yes'
    const run = await loadMiddleware()

    run({ path: '/client/invoices', fullPath: '/client/invoices' })

    expect(navigateToMock).not.toHaveBeenCalled()
  })
})
