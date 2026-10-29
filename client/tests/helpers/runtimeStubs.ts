/**
 * Stand-ins for the Nuxt route auto-imports that every page-level test helper needs.
 *
 * The client, public-page and auth helpers each stood in for `useRoute` with the same object
 * literal; this module is the one definition they share.
 *
 * @module tests/helpers/runtimeStubs
 */

import { vi } from 'vitest'

/** What a test can change about the stood-in route. */
export interface RouteStubOptions {
  /** The route parameters (`route.params`). */
  params?: Record<string, string>
  /** The route query (`route.query`). */
  query?: Record<string, string | string[]>
  /** The full path (`route.fullPath`). */
  fullPath?: string
}

/**
 * Defines `useRoute` as a global returning a fixed route.
 *
 * @param options Parameters, query and full path of the route.
 */
export const stubRoute = (options: RouteStubOptions = {}): void => {
  vi.stubGlobal('useRoute', () => ({
    params: options.params ?? {},
    query: options.query ?? {},
    path: '/',
    fullPath: options.fullPath ?? '/',
    meta: {},
  }))
}

/**
 * Defines `useLocalePath` as a global that prefixes every path.
 *
 * @param prefix What to put before the path; empty keeps the path as given.
 */
export const stubLocalePath = (prefix: string = ''): void => {
  vi.stubGlobal('useLocalePath', () => (path: string) => `${prefix}${path}`)
}
