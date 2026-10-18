/**
 * Unit tests for ClientVerifyEmailBanner: it shows only for an explicitly unconfirmed address,
 * and its button asks the auth store to send the mail again, showing the sentence that results.
 *
 * @module tests/components/client/VerifyEmailBanner.test
 */

import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountClient } from '~/tests/helpers/mountClient'

/** The mocked status read, re-created for every test. */
const fetchEmailVerified = vi.fn()

/** The mocked resend call, re-created for every test. */
const resendVerification = vi.fn()

vi.mock('~/composables/apis/useAuthApi', () => ({
  /**
   * Stands in for the real API composable so the store runs without transport.
   *
   * @returns Only the two endpoints the banner reaches.
   */
  useAuthApi: () => ({ fetchEmailVerified, resendVerification })
}))

const { default: VerifyEmailBanner } = await import('~/components/client/VerifyEmailBanner.vue')

/**
 * Mounts the banner and waits for the status read.
 *
 * @returns The wrapper.
 */
const mountBanner = async () => {
  const wrapper = mountClient(VerifyEmailBanner)
  await flushPromises()
  return wrapper
}

describe('ClientVerifyEmailBanner', () => {
  beforeEach(() => {
    fetchEmailVerified.mockReset()
    resendVerification.mockReset()
  })

  it('is shown when the address is not confirmed', async () => {
    fetchEmailVerified.mockResolvedValue({ verified: false })

    const wrapper = await mountBanner()

    expect(wrapper.find('[data-testid="verify-email-banner"]').exists()).toBe(true)
  })

  it('is hidden when the address is confirmed', async () => {
    fetchEmailVerified.mockResolvedValue({ verified: true })

    const wrapper = await mountBanner()

    expect(wrapper.find('[data-testid="verify-email-banner"]').exists()).toBe(false)
  })

  it('is hidden when the answer is unknown, so a missing claim never nags', async () => {
    fetchEmailVerified.mockResolvedValue({ verified: null })

    const wrapper = await mountBanner()

    expect(wrapper.find('[data-testid="verify-email-banner"]').exists()).toBe(false)
  })

  it('asks for the mail again and confirms it was sent', async () => {
    fetchEmailVerified.mockResolvedValue({ verified: false })
    resendVerification.mockResolvedValue(undefined)
    const wrapper = await mountBanner()

    await wrapper.find('[data-testid="verify-email-resend"]').trigger('click')
    await flushPromises()

    expect(resendVerification).toHaveBeenCalledTimes(1)
    expect(wrapper.find('[data-testid="verify-email-message"]').text()).toContain('client.verifyEmail.sent')
  })

  it('shows the backend sentence when the resend is refused', async () => {
    fetchEmailVerified.mockResolvedValue({ verified: false })
    resendVerification.mockRejectedValue({ data: { detail: 'Too many requests.' } })
    const wrapper = await mountBanner()

    await wrapper.find('[data-testid="verify-email-resend"]').trigger('click')
    await flushPromises()

    expect(wrapper.find('[data-testid="verify-email-message"]').text()).toBe('Too many requests.')
  })
})
