/**
 * Tests for `pages/client/confirm-email.vue` against the real auth store: the three panels the
 * confirmation walks through. Only the API composable is replaced.
 *
 * @module tests/pages/client/confirm-email.test
 */

import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountAuthPage, stubAuthRuntime } from '~/tests/helpers/authScreens'

/** The mocked email-confirmation call, re-created for every test. */
const confirmEmail = vi.fn()

vi.mock('~/composables/apis/useAuthApi', () => ({
  /**
   * Stands in for the real API composable so the store runs without transport.
   *
   * @returns Only the endpoint this page reaches through the store.
   */
  useAuthApi: () => ({ confirmEmail })
}))

const { default: ConfirmEmailPage } = await import('~/pages/client/confirm-email.vue')

beforeEach(() => {
  confirmEmail.mockReset()
})

describe('client confirm-email page', () => {
  it('confirms the address from the link and offers sign-in', async () => {
    stubAuthRuntime({ query: { email: 'ada@example.com', token: 'tok' } })
    confirmEmail.mockResolvedValue({})

    const wrapper = await mountAuthPage(ConfirmEmailPage)
    await flushPromises()

    expect(confirmEmail).toHaveBeenCalledWith('ada@example.com', 'tok')
    expect(wrapper.find('[data-testid="confirm-success"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="confirm-signin"]').attributes('href')).toBe('/client/login')
  })

  it('shows the API sentence and a way back to registration when it is refused', async () => {
    stubAuthRuntime({ query: { email: 'ada@example.com', token: 'tok' } })
    confirmEmail.mockRejectedValue({ data: { detail: 'This link has expired.' } })

    const wrapper = await mountAuthPage(ConfirmEmailPage)
    await flushPromises()

    expect(wrapper.find('[data-testid="confirm-failed"]').text()).toContain('This link has expired.')
    expect(wrapper.find('[data-testid="confirm-register"]').attributes('href')).toBe('/client/register')
  })

  it('reports an incomplete link without calling the API', async () => {
    stubAuthRuntime({ query: { email: 'ada@example.com' } })

    const wrapper = await mountAuthPage(ConfirmEmailPage)
    await flushPromises()

    expect(confirmEmail).not.toHaveBeenCalled()
    expect(wrapper.find('[data-testid="confirm-failed"]').text()).toContain('client.confirmEmail.invalidLink')
  })

  it('has no logo or heading of its own, only the outcome', async () => {
    stubAuthRuntime({ query: { email: 'ada@example.com', token: 'tok' } })
    confirmEmail.mockResolvedValue({})

    const wrapper = await mountAuthPage(ConfirmEmailPage)

    expect(wrapper.find('[data-testid="auth-logo"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="auth-shell-title"]').exists()).toBe(false)
  })
})
