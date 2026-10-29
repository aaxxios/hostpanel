/**
 * Tests for `pages/client/confirm-email-change.vue` against the real account and auth stores: the
 * three panels the confirmation of a sign-in address change walks through. Only the API
 * composables are replaced.
 *
 * @module tests/pages/client/confirm-email-change.test
 */

import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountAuthPage, stubAuthRuntime } from '~/tests/helpers/authScreens'

/** The mocked confirmation call, re-created for every test. */
const confirmEmailChange = vi.fn()

/** The mocked session clear, re-created for every test. */
const logout = vi.fn()

vi.mock('~/composables/apis/useClientApi', () => ({
  /**
   * Stands in for the client API composable so the store runs without transport.
   *
   * @returns Only the endpoint this page reaches through the store.
   */
  useClientApi: () => ({ confirmEmailChange })
}))

vi.mock('~/composables/apis/useBillingApi', () => ({
  /**
   * Stands in for the billing API composable, which this page never reaches.
   *
   * @returns No endpoints.
   */
  useBillingApi: () => ({})
}))

vi.mock('~/composables/apis/useAuthApi', () => ({
  /**
   * Stands in for the auth API composable.
   *
   * @returns Only the session clear the confirmation ends with.
   */
  useAuthApi: () => ({ logout })
}))

const { default: ConfirmEmailChangePage } = await import('~/pages/client/confirm-email-change.vue')

beforeEach(() => {
  confirmEmailChange.mockReset()
  logout.mockReset().mockResolvedValue(undefined)
})

describe('client confirm-email-change page', () => {
  it('confirms the change from the link, ends this session and offers sign-in', async () => {
    stubAuthRuntime({ query: { email: 'new@example.com', token: 'tok' } })
    confirmEmailChange.mockResolvedValue(undefined)

    const wrapper = await mountAuthPage(ConfirmEmailChangePage)
    await flushPromises()

    expect(confirmEmailChange).toHaveBeenCalledWith('new@example.com', 'tok')
    expect(logout).toHaveBeenCalledTimes(1)
    expect(wrapper.find('[data-testid="confirm-email-change-success"]').text()).toContain('client.emailChange.signInAgain')
    expect(wrapper.find('[data-testid="confirm-email-change-signin"]').attributes('href')).toBe('/client/login')
  })

  it('shows the API sentence and a way back to the account when it is refused', async () => {
    stubAuthRuntime({ query: { email: 'new@example.com', token: 'tok' } })
    confirmEmailChange.mockRejectedValue({ data: { detail: 'This link has expired.' } })

    const wrapper = await mountAuthPage(ConfirmEmailChangePage)
    await flushPromises()

    expect(wrapper.find('[data-testid="confirm-email-change-failed"]').text()).toContain('This link has expired.')
    expect(wrapper.find('[data-testid="confirm-email-change-account"]').attributes('href')).toBe('/client/account')
    expect(logout).not.toHaveBeenCalled()
  })

  it('reports an incomplete link without calling the API', async () => {
    stubAuthRuntime({ query: { email: 'new@example.com' } })

    const wrapper = await mountAuthPage(ConfirmEmailChangePage)
    await flushPromises()

    expect(confirmEmailChange).not.toHaveBeenCalled()
    expect(wrapper.find('[data-testid="confirm-email-change-failed"]').text()).toContain('client.confirmEmail.invalidLink')
  })
})
