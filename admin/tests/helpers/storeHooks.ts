/**
 * Shared setup for store tests: a fresh Pinia per test, `fetch` stubs removed afterwards, and
 * the sentence the client writes when nothing answered.
 */

import { afterEach, beforeEach, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

/** Sentence the client writes when nothing answered. */
export const NO_ANSWER = 'Could not reach the server. Check your connection and try again.'

/**
 * Registers the hooks every store suite needs in the enclosing `describe`: activates a new
 * Pinia before each test and removes stubbed globals (`fetch`) after each one.
 */
export const installStoreHooks = (): void => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })
}
