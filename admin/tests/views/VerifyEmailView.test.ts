/**
 * Mount tests for VerifyEmailView: a link without a token is refused locally, a verified link
 * shows success, and a refusal shows the API's sentence.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import VerifyEmailView from '@/views/VerifyEmailView.vue'
import { mountRouted } from '../helpers/mount'
import { stubRoutes } from '../helpers/routes'
import { problem } from '../helpers/fetch'

describe('VerifyEmailView', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('refuses a link with no token or address', async () => {
    stubRoutes({})
    const { wrapper } = await mountRouted(VerifyEmailView, '/verify-email', '/verify-email')
    await flushPromises()

    expect(wrapper.find('[data-testid="verify-email-failure"]').exists()).toBe(true)
    expect(wrapper.get('[data-testid="verify-email-error"]').text()).not.toBe('')
  })

  it('shows success once the API accepts the token', async () => {
    stubRoutes({ 'POST /api/auth/confirm-email': () => new Response(null, { status: 200 }) })
    const { wrapper } = await mountRouted(VerifyEmailView, '/verify-email', '/verify-email?token=t&email=a%40b.com')
    await flushPromises()

    expect(wrapper.find('[data-testid="verify-email-success"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="verify-email-go-login"]').exists()).toBe(true)
  })

  it('shows the API sentence when the token is refused', async () => {
    stubRoutes({ 'POST /api/auth/confirm-email': () => problem('The link has expired.', 400) })
    const { wrapper } = await mountRouted(VerifyEmailView, '/verify-email', '/verify-email?token=t&email=a%40b.com')
    await flushPromises()

    expect(wrapper.get('[data-testid="verify-email-error"]').text()).toContain('The link has expired.')
  })
})
