/**
 * Tests for `pages/client/reset-password.vue` against the real auth store: the invalid-link
 * state, the mismatch check, the API call and its outcomes. Only the API composable is replaced.
 *
 * @module tests/pages/client/reset-password.test
 */

import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountAuthPage, stubAuthRuntime } from '~/tests/helpers/authScreens'

/** The mocked password-reset call, re-created for every test. */
const resetPassword = vi.fn()

vi.mock('~/composables/apis/useAuthApi', () => ({
  /**
   * Stands in for the real API composable so the store runs without transport.
   *
   * @returns Only the endpoint this page reaches through the store.
   */
  useAuthApi: () => ({ resetPassword })
}))

const { default: ResetPasswordPage } = await import('~/pages/client/reset-password.vue')

/** A valid reset link's query. */
const LINK = { token: 'tok', email: 'ada@example.com' }

/**
 * Types both password fields and submits.
 *
 * @param wrapper - The mounted page.
 * @param first - New password.
 * @param second - Confirmation.
 */
const submit = async (wrapper: Awaited<ReturnType<typeof mountAuthPage>>, first: string, second: string): Promise<void> => {
  await wrapper.find('[data-testid="reset-new-password"]').setValue(first)
  await wrapper.find('[data-testid="reset-confirm-password"]').setValue(second)
  await wrapper.find('form').trigger('submit')
  await flushPromises()
}

beforeEach(() => {
  resetPassword.mockReset()
})

describe('client reset-password page', () => {
  it.each([
    ['no token', { email: 'ada@example.com' }],
    ['no address', { token: 'tok' }],
    ['neither', {}]
  ])('shows the invalid-link state with %s', async (_name, query) => {
    stubAuthRuntime({ query })

    const wrapper = await mountAuthPage(ResetPasswordPage)

    expect(wrapper.find('[data-testid="reset-invalid"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="reset-form"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="reset-request-new"]').attributes('href')).toBe('/client/forgot')
  })

  it('shows the form with placeholders for a valid link', async () => {
    stubAuthRuntime({ query: LINK })

    const wrapper = await mountAuthPage(ResetPasswordPage)

    expect(wrapper.find('[data-testid="reset-new-password"]').attributes('placeholder')).toBe('client.resetPassword.newPasswordPlaceholder')
    expect(wrapper.find('[data-testid="reset-confirm-password"]').attributes('placeholder')).toBe('client.resetPassword.confirmPasswordPlaceholder')
  })

  it('refuses mismatching passwords without calling the API', async () => {
    stubAuthRuntime({ query: LINK })
    const wrapper = await mountAuthPage(ResetPasswordPage)

    await submit(wrapper, 'one-password', 'another-password')

    expect(wrapper.find('[data-testid="ui-form-error"]').text()).toBe('client.resetPassword.errorMismatch')
    expect(resetPassword).not.toHaveBeenCalled()
  })

  it('sends the address, token and new password and shows the success state', async () => {
    stubAuthRuntime({ query: LINK })
    resetPassword.mockResolvedValue({})
    const wrapper = await mountAuthPage(ResetPasswordPage)

    await submit(wrapper, 'new-password-1', 'new-password-1')

    expect(resetPassword).toHaveBeenCalledWith('ada@example.com', 'tok', 'new-password-1')
    expect(wrapper.find('[data-testid="reset-success"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="reset-success-signin"]').attributes('href')).toBe('/client/login')
  })

  it('shows the API sentence and keeps the form when the reset is refused', async () => {
    stubAuthRuntime({ query: LINK })
    resetPassword.mockRejectedValue({ data: { detail: 'This link has expired.' } })
    const wrapper = await mountAuthPage(ResetPasswordPage)

    await submit(wrapper, 'new-password-1', 'new-password-1')

    expect(wrapper.find('[data-testid="ui-form-error"]').text()).toBe('This link has expired.')
    expect(wrapper.find('[data-testid="reset-success"]').exists()).toBe(false)
  })
})
