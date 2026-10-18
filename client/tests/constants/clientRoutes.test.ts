/**
 * Unit tests for `constants/clientRoutes.ts`.
 *
 * @module tests/constants/clientRoutes.test
 */

import { describe, expect, it } from 'vitest'
import {
  CLIENT_DASHBOARD_PATH,
  CLIENT_LOGIN_PATH,
  CLIENT_PUBLIC_ROUTES,
  CLIENT_REDIRECT_QUERY,
} from '~/constants/clientRoutes'

describe('client routes', () => {
  it('keeps the well-known paths', () => {
    expect(CLIENT_LOGIN_PATH).toBe('/client/login')
    expect(CLIENT_DASHBOARD_PATH).toBe('/client/dashboard')
    expect(CLIENT_REDIRECT_QUERY).toBe('redirect')
  })

  it('lists the sign-in page among the public routes and never the dashboard', () => {
    expect(CLIENT_PUBLIC_ROUTES).toContain(CLIENT_LOGIN_PATH)
    expect(CLIENT_PUBLIC_ROUTES).not.toContain(CLIENT_DASHBOARD_PATH)
  })

  it('keeps every public route under the client area', () => {
    expect(CLIENT_PUBLIC_ROUTES.every(route => route.startsWith('/client/'))).toBe(true)
  })
})
