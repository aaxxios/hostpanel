/**
 * Tests for `plugins/client-auth.ts`: the profile is fetched once after hydration, and only
 * when a session exists.
 *
 * @module tests/plugins/client-auth.test
 */

import { beforeEach, describe, expect, it, vi } from 'vitest'

/** The store's profile fetch. */
const fetchUser = vi.fn()

/** Whether the stubbed store reports a session. */
let isLoggedIn = false

/**
 * Loads the plugin and runs it.
 */
const runPlugin = async (): Promise<void> => {
  vi.stubGlobal('defineNuxtPlugin', (setup: () => Promise<void>) => setup)
  vi.stubGlobal('useAuthStore', () => ({ isLoggedIn, fetchUser }))
  /** The plugin as Nuxt calls it. */
  const plugin = (await import('~/plugins/client-auth')).default as unknown as () => Promise<void>
  await plugin()
}

beforeEach(() => {
  fetchUser.mockReset()
  isLoggedIn = false
  vi.resetModules()
})

describe('client-auth plugin', () => {
  it('fetches the profile when a session exists', async () => {
    isLoggedIn = true

    await runPlugin()

    expect(fetchUser).toHaveBeenCalledTimes(1)
  })

  it('does not call the API for an anonymous visitor', async () => {
    await runPlugin()

    expect(fetchUser).not.toHaveBeenCalled()
  })
})
