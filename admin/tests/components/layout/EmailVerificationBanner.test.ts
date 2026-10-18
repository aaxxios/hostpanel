/**
 * Mount tests for EmailVerificationBanner: shown only while the account's email is known to be
 * unverified.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { mount } from '@vue/test-utils'
import EmailVerificationBanner from '@/components/layout/EmailVerificationBanner.vue'
import { useAuthStore } from '@/stores/auth'
import { createTestI18n } from '../../helpers/i18n'
import { problem, stubFetch } from '../../helpers/fetch'

/**
 * Mounts the banner with the given verification state.
 *
 * @param emailVerified - The auth store's state: true, false, or null (not checked yet).
 * @returns The wrapper.
 */
const mountBanner = (emailVerified: boolean | null) => {
  const pinia = createPinia()
  setActivePinia(pinia)
  useAuthStore().emailVerified = emailVerified
  return mount(EmailVerificationBanner, { global: { plugins: [createTestI18n(), pinia] } })
}

describe('EmailVerificationBanner', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('shows the message while the email is unverified', () => {
    const wrapper = mountBanner(false)

    expect(wrapper.get('[data-testid="email-verification-banner"]').text()).toContain('Verify Your Email')
  })

  it('stays hidden once the email is verified', () => {
    expect(mountBanner(true).find('[data-testid="email-verification-banner"]').exists()).toBe(false)
  })

  it('stays hidden before the state is known', () => {
    expect(mountBanner(null).find('[data-testid="email-verification-banner"]').exists()).toBe(false)
  })

  it('resends the verification mail and confirms it', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))
    const wrapper = mountBanner(false)

    await wrapper.get('[data-testid="email-verification-resend"]').trigger('click')
    await flushPromises()

    expect(mock.mock.calls[0]![0]).toBe('/api/auth/resend-verification')
    expect(wrapper.find('[data-testid="email-verification-resent"]').exists()).toBe(true)
  })

  it('shows the API sentence when the resend is refused', async () => {
    stubFetch(problem('Too many requests.', 429))
    const wrapper = mountBanner(false)

    await wrapper.get('[data-testid="email-verification-resend"]').trigger('click')
    await flushPromises()

    expect(wrapper.get('[data-testid="email-verification-error"]').text()).toContain('Too many requests.')
  })
})
