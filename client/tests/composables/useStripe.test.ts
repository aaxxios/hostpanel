/**
 * Tests for `useStripe`: one lazy Stripe.js load shared by every caller, and a refusal when the
 * publishable key is not configured.
 *
 * @module tests/composables/useStripe.test
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

/** The mocked vendor loader. */
const { loadStripe } = vi.hoisted(() => ({ loadStripe: vi.fn() }))

vi.mock('@stripe/stripe-js', () => ({ loadStripe }))

/**
 * Loads a fresh copy of the composable, so its module-level cache starts empty.
 *
 * @param key The publishable key in runtime config.
 * @returns The composable's public surface.
 */
const freshStripe = async (key: string | undefined) => {
  vi.resetModules()
  vi.stubGlobal('useRuntimeConfig', () => ({ public: { stripePublishableKey: key } }))
  const { useStripe } = await import('~/composables/useStripe')
  return useStripe()
}

describe('useStripe', () => {
  beforeEach(() => {
    loadStripe.mockReset()
    loadStripe.mockResolvedValue({ id: 'stripe-instance' })
  })

  afterEach(() => {
    vi.unstubAllGlobals()
    vi.restoreAllMocks()
  })

  it('loads Stripe.js with the configured key', async () => {
    const { getStripe } = await freshStripe('pk_test_1')

    await expect(getStripe()).resolves.toEqual({ id: 'stripe-instance' })
    expect(loadStripe).toHaveBeenCalledWith('pk_test_1')
  })

  it('loads the script once however many times it is asked for', async () => {
    const { getStripe } = await freshStripe('pk_test_1')

    const first = getStripe()
    const second = getStripe()

    expect(second).toBe(first)
    expect(loadStripe).toHaveBeenCalledTimes(1)
  })

  it('resolves null and logs when the key is missing', async () => {
    const error = vi.spyOn(console, 'error').mockImplementation(() => undefined)
    const { getStripe } = await freshStripe('')

    await expect(getStripe()).resolves.toBeNull()
    expect(loadStripe).not.toHaveBeenCalled()
    expect(error).toHaveBeenCalled()
  })
})
