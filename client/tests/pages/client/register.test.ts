/**
 * Tests for `pages/client/register.vue` against the real auth store: the registration form, its
 * refusals and the redirect to sign-in. Only the API composable is replaced.
 *
 * @module tests/pages/client/register.test
 */

import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountAuthPage, stubAuthRuntime } from '~/tests/helpers/authScreens'

/** The mocked registration call, re-created for every test. */
const register = vi.fn()

vi.mock('~/composables/apis/useAuthApi', () => ({
  /**
   * Stands in for the real API composable so the store runs without transport.
   *
   * @returns Only the endpoint this page reaches through the store.
   */
  useAuthApi: () => ({ register })
}))

const { default: RegisterPage } = await import('~/pages/client/register.vue')

/**
 * Fills every field and submits the form.
 *
 * @param wrapper - The mounted page.
 */
const submit = async (wrapper: Awaited<ReturnType<typeof mountAuthPage>>): Promise<void> => {
  await wrapper.find('[data-testid="register-firstname"]').setValue('Ada')
  await wrapper.find('[data-testid="register-lastname"]').setValue('Lovelace')
  await wrapper.find('[data-testid="register-email"]').setValue('ada@example.com')
  await wrapper.find('[data-testid="register-password"]').setValue('long-enough-pw')
  await wrapper.find('form').trigger('submit')
  await flushPromises()
}

beforeEach(() => {
  register.mockReset()
  stubAuthRuntime()
})

describe('client register page', () => {
  it('gives every field a placeholder', async () => {
    const wrapper = await mountAuthPage(RegisterPage)

    for (const id of ['firstname', 'lastname', 'email', 'password']) {
      expect(wrapper.find(`[data-testid="register-${id}"]`).attributes('placeholder')).toMatch(/^client\.register\./)
    }
  })

  it('sends the typed fields to the API', async () => {
    register.mockResolvedValue({})
    const wrapper = await mountAuthPage(RegisterPage)

    await submit(wrapper)

    expect(register).toHaveBeenCalledWith({ firstname: 'Ada', lastname: 'Lovelace', email: 'ada@example.com', password: 'long-enough-pw' })
  })

  it('shows the success banner and schedules the redirect to sign-in', async () => {
    register.mockResolvedValue({})
    const timer = vi.spyOn(globalThis, 'setTimeout')
    const wrapper = await mountAuthPage(RegisterPage)

    await submit(wrapper)

    expect(wrapper.find('[data-testid="ui-form-success"]').text()).toBe('client.register.success')
    expect(timer).toHaveBeenCalledWith(expect.any(Function), 1500)
    timer.mockRestore()
  })

  it('shows the API sentence and no success banner when registration is refused', async () => {
    register.mockRejectedValue({ data: { detail: 'That email is taken.' } })
    const wrapper = await mountAuthPage(RegisterPage)

    await submit(wrapper)

    expect(wrapper.find('[data-testid="ui-form-error"]').text()).toBe('That email is taken.')
    expect(wrapper.find('[data-testid="ui-form-success"]').exists()).toBe(false)
  })

  it('shows a field message under its field', async () => {
    register.mockRejectedValue({ data: { errors: { password: ['Too short.'] } } })
    const wrapper = await mountAuthPage(RegisterPage)

    await submit(wrapper)

    expect(wrapper.find('[data-testid="register-password"]').element.closest('div.w-full')?.textContent).toContain('Too short.')
  })

  it('links back to sign-in', async () => {
    const wrapper = await mountAuthPage(RegisterPage)

    expect(wrapper.find('[data-testid="register-signin"]').attributes('href')).toBe('/client/login')
  })
})
