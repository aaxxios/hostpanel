/**
 * Tests for `pages/client/accept-invite.vue` against the real auth store: the missing-token
 * state, the sign-in-required step and a plain refusal. Only the API composable is replaced.
 *
 * @module tests/pages/client/accept-invite.test
 */

import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountAuthPage, stubAuthRuntime } from '~/tests/helpers/authScreens'

/** The mocked accept-invitation call, re-created for every test. */
const acceptInvite = vi.fn()

vi.mock('~/composables/apis/useAuthApi', () => ({
  /**
   * Stands in for the real API composable so the store runs without transport.
   *
   * @returns Only the endpoint this page reaches through the store.
   */
  useAuthApi: () => ({ acceptInvite })
}))

const { default: AcceptInvitePage } = await import('~/pages/client/accept-invite.vue')

beforeEach(() => {
  acceptInvite.mockReset()
})

describe('client accept-invite page', () => {
  it('shows the invalid-link state when the link has no token', async () => {
    stubAuthRuntime()

    const wrapper = await mountAuthPage(AcceptInvitePage)

    expect(wrapper.find('[data-testid="invite-invalid"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="invite-form"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="invite-invalid-back"]').attributes('href')).toBe('/client/login')
  })

  it('shows the accept form for a link with a token', async () => {
    stubAuthRuntime({ query: { token: 'tok' } })

    const wrapper = await mountAuthPage(AcceptInvitePage)

    expect(wrapper.find('[data-testid="invite-form"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="invite-submit"]').exists()).toBe(true)
  })

  it('posts the token when the form is submitted', async () => {
    stubAuthRuntime({ query: { token: 'tok' } })
    acceptInvite.mockRejectedValue({ data: { detail: 'This invitation has expired.' } })
    const wrapper = await mountAuthPage(AcceptInvitePage)

    await wrapper.find('form').trigger('submit')
    await flushPromises()

    expect(acceptInvite).toHaveBeenCalledWith('tok')
  })

  it('shows the API sentence for a refused invitation', async () => {
    stubAuthRuntime({ query: { token: 'tok' } })
    acceptInvite.mockRejectedValue({ data: { detail: 'This invitation has expired.' } })
    const wrapper = await mountAuthPage(AcceptInvitePage)

    await wrapper.find('form').trigger('submit')
    await flushPromises()

    expect(wrapper.find('[data-testid="ui-form-error"]').text()).toBe('This invitation has expired.')
  })

  it('offers the sign-in step when the API says a session is needed', async () => {
    stubAuthRuntime({ query: { token: 'tok' } })
    acceptInvite.mockRejectedValue({ data: { type: 'INVITE_SIGN_IN_REQUIRED', detail: 'Sign in first.' } })
    const wrapper = await mountAuthPage(AcceptInvitePage)

    await wrapper.find('form').trigger('submit')
    await flushPromises()

    expect(wrapper.find('[data-testid="invite-sign-in-required"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="invite-form"]').exists()).toBe(false)
    expect(wrapper.text()).toContain('client.acceptInvite.signInRequired')
  })

  it('shows the accept form again when the visitor returns to the link after a sign-in-required refusal', async () => {
    stubAuthRuntime({ query: { token: 'tok' } })
    acceptInvite.mockRejectedValue({ data: { type: 'INVITE_SIGN_IN_REQUIRED' } })
    const first = await mountAuthPage(AcceptInvitePage)
    await first.find('form').trigger('submit')
    await flushPromises()
    expect(first.find('[data-testid="invite-sign-in-required"]').exists()).toBe(true)

    const second = await mountAuthPage(AcceptInvitePage)

    expect(second.find('[data-testid="invite-form"]').exists()).toBe(true)
    expect(second.find('[data-testid="invite-sign-in-required"]').exists()).toBe(false)
  })

  it('sends the visitor to the local sign-in page and back to this exact link', async () => {
    stubAuthRuntime({ query: { token: 'tok' }, fullPath: '/client/accept-invite?token=tok' })
    acceptInvite.mockRejectedValue({ data: { type: 'INVITE_SIGN_IN_REQUIRED' } })
    const wrapper = await mountAuthPage(AcceptInvitePage)
    await wrapper.find('form').trigger('submit')
    await flushPromises()

    await wrapper.find('[data-testid="invite-sign-in"]').trigger('click')

    expect(navigateTo).toHaveBeenCalledWith(`/client/login?redirect=${encodeURIComponent('/client/accept-invite?token=tok')}`)
  })

  it('sends the visitor to the identity provider under SSO, with the return address', async () => {
    stubAuthRuntime({
      query: { token: 'tok' },
      fullPath: '/client/accept-invite?token=tok',
      isSso: true,
      publicConfig: { baseUrl: 'https://portal.example.test' }
    })
    acceptInvite.mockRejectedValue({ data: { type: 'INVITE_SIGN_IN_REQUIRED' } })
    const wrapper = await mountAuthPage(AcceptInvitePage)
    await wrapper.find('form').trigger('submit')
    await flushPromises()

    await wrapper.find('[data-testid="invite-sign-in"]').trigger('click')

    expect(navigateTo).toHaveBeenCalledWith(
      `https://portal.example.test/api/portal/auth/sso/authorize?returnTo=${encodeURIComponent('/client/accept-invite?token=tok')}`,
      { external: true }
    )
  })
})
