/**
 * Unit tests for ClientForgotPasswordPanel against the real auth store: the request form, its
 * refusal and the "mail sent" state. Only the API composable is replaced.
 *
 * @module tests/components/client/ForgotPasswordPanel.test
 */

import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountWithLogo } from '~/tests/helpers/authScreens'

/** The mocked reset-request call, re-created for every test. */
const requestPasswordReset = vi.fn()

vi.mock('~/composables/apis/useAuthApi', () => ({
  /**
   * Stands in for the real API composable so the store runs without transport.
   *
   * @returns Only the endpoint this component reaches through the store.
   */
  useAuthApi: () => ({ requestPasswordReset })
}))

const { default: ForgotPasswordPanel } = await import('~/components/client/ForgotPasswordPanel.vue')

/**
 * Types an address and submits the form.
 *
 * @param wrapper - The mounted panel.
 * @param email - Address to type.
 */
const submitWith = async (wrapper: ReturnType<typeof mountWithLogo>, email: string): Promise<void> => {
  await wrapper.find('[data-testid="forgot-email"]').setValue(email)
  await wrapper.find('form').trigger('submit')
  await flushPromises()
}

beforeEach(() => {
  requestPasswordReset.mockReset()
})

describe('ClientForgotPasswordPanel', () => {
  it('shows the form with a placeholder on the email field', () => {
    const wrapper = mountWithLogo(ForgotPasswordPanel)

    expect(wrapper.find('[data-testid="forgot-form"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="forgot-email"]').attributes('placeholder')).toBe('client.forgot.emailPlaceholder')
  })

  it('asks the API for a reset mail and switches to the sent state, echoing the address', async () => {
    requestPasswordReset.mockResolvedValue({})
    const wrapper = mountWithLogo(ForgotPasswordPanel)

    await submitWith(wrapper, 'ada@example.com')

    expect(requestPasswordReset).toHaveBeenCalledWith('ada@example.com')
    expect(wrapper.find('[data-testid="forgot-form"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="outcome-panel"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="forgot-sent-back"]').attributes('href')).toBe('/client/login')
  })

  it('keeps the form and shows the API sentence when the request is refused', async () => {
    requestPasswordReset.mockRejectedValue({ data: { detail: 'Too many requests.' } })
    const wrapper = mountWithLogo(ForgotPasswordPanel)

    await submitWith(wrapper, 'ada@example.com')

    expect(wrapper.find('[data-testid="forgot-form"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="ui-form-error"]').text()).toBe('Too many requests.')
  })

  it('links back to sign-in from the form', () => {
    const wrapper = mountWithLogo(ForgotPasswordPanel)

    expect(wrapper.find('[data-testid="forgot-back"]').attributes('href')).toBe('/client/login')
  })
})
