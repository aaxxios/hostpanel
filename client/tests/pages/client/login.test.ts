/**
 * Tests for `pages/client/login.vue` against the real auth store: the local form, its refusals,
 * the redirect after sign-in and the single SSO button. Only the API composable is replaced.
 *
 * @module tests/pages/client/login.test
 */

import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { APP_LOGO_STUB, mountAuthPage, stubAuthRuntime } from '~/tests/helpers/authScreens'
import { mountPage } from '~/tests/helpers/mountClient'
import { useDemoStore } from '~/stores/demo'
import { DemoAccountRole } from '~/types/demoaccountrole'

/** The mocked password sign-in call, re-created for every test. */
const login = vi.fn()

/** The mocked second-factor sign-in call, re-created for every test. */
const loginWithTwoFactor = vi.fn()

vi.mock('~/composables/apis/useAuthApi', () => ({
  /**
   * Stands in for the real API composable so the store runs without transport.
   *
   * @returns Only the endpoint this page reaches through the store.
   */
  useAuthApi: () => ({ login, loginWithTwoFactor })
}))

const { default: LoginPage } = await import('~/pages/client/login.vue')

/**
 * Types the credentials and submits the form.
 *
 * @param wrapper - The mounted page.
 * @param password - Password to type.
 */
const signIn = async (wrapper: Awaited<ReturnType<typeof mountAuthPage>>, password = 'secret'): Promise<void> => {
  await wrapper.find('[data-testid="login-email"]').setValue('ada@example.com')
  await wrapper.find('[data-testid="login-password"]').setValue(password)
  await wrapper.find('form').trigger('submit')
  await flushPromises()
}

beforeEach(() => {
  login.mockReset()
  loginWithTwoFactor.mockReset()
})

describe('client login page (local mode)', () => {
  it('shows both fields with placeholders and the submit button', async () => {
    stubAuthRuntime()
    const wrapper = await mountAuthPage(LoginPage)

    expect(wrapper.find('[data-testid="login-email"]').attributes('placeholder')).toBe('client.login.emailPlaceholder')
    expect(wrapper.find('[data-testid="login-password"]').attributes('placeholder')).toBe('client.login.passwordPlaceholder')
    expect(wrapper.find('[data-testid="login-submit"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="login-sso"]').exists()).toBe(false)
  })

  it('links to password recovery and registration', async () => {
    stubAuthRuntime()
    const wrapper = await mountAuthPage(LoginPage)

    expect(wrapper.find('[data-testid="login-forgot"]').attributes('href')).toBe('/client/forgot-password')
    expect(wrapper.find('[data-testid="login-register"]').attributes('href')).toBe('/client/register')
  })

  it('signs in and goes to the dashboard', async () => {
    const { push } = stubAuthRuntime()
    login.mockResolvedValue({ id: 1, email: 'ada@example.com' })
    const wrapper = await mountAuthPage(LoginPage)

    await signIn(wrapper)

    expect(login).toHaveBeenCalledWith('ada@example.com', 'secret')
    expect(push).toHaveBeenCalledWith('/client/dashboard')
  })

  it('goes back to the page the visitor asked for', async () => {
    const { push } = stubAuthRuntime({ query: { redirect: '/client/invoices/7' } })
    login.mockResolvedValue({ id: 1, email: 'ada@example.com' })
    const wrapper = await mountAuthPage(LoginPage)

    await signIn(wrapper)

    expect(push).toHaveBeenCalledWith('/client/invoices/7')
  })

  it('ignores a redirect that leaves the app and goes to the dashboard', async () => {
    const { push } = stubAuthRuntime({ query: { redirect: 'https://evil.example/phish' } })
    login.mockResolvedValue({ id: 1, email: 'ada@example.com' })
    const wrapper = await mountAuthPage(LoginPage)

    await signIn(wrapper)

    expect(push).toHaveBeenCalledWith('/client/dashboard')
  })

  it('shows the API sentence and stays put when the credentials are refused', async () => {
    const { push } = stubAuthRuntime()
    login.mockRejectedValue({ data: { detail: 'Wrong email or password.' } })
    const wrapper = await mountAuthPage(LoginPage)

    await signIn(wrapper, 'nope')

    expect(wrapper.find('[data-testid="login-error"]').text()).toBe('Wrong email or password.')
    expect(push).not.toHaveBeenCalled()
  })

  it('shows a field message under the field it belongs to', async () => {
    stubAuthRuntime()
    login.mockRejectedValue({ data: { errors: { email: ['Enter a valid email.'] } } })
    const wrapper = await mountAuthPage(LoginPage)

    await signIn(wrapper)

    expect(wrapper.text()).toContain('Enter a valid email.')
  })

  it('swaps the password form for the code step when a second factor is required', async () => {
    const { push } = stubAuthRuntime()
    login.mockResolvedValue({ twoFactorRequired: true, pendingToken: 'p' })
    const wrapper = await mountAuthPage(LoginPage)

    await signIn(wrapper)

    expect(wrapper.find('[data-testid="login-form"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="login-2fa-form"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="login-2fa-submit"]').attributes('disabled')).toBeDefined()
    expect(push).not.toHaveBeenCalled()
  })

  it('finishes the sign-in with the typed code and follows the return path', async () => {
    const { push } = stubAuthRuntime()
    login.mockResolvedValue({ twoFactorRequired: true, pendingToken: 'p' })
    loginWithTwoFactor.mockResolvedValue({ id: 1 })
    const wrapper = await mountAuthPage(LoginPage)
    await signIn(wrapper)

    for (const [index, digit] of ['1', '2', '3', '4', '5', '6'].entries()) {
      await wrapper.find(`[data-testid="otp-input-${index}"]`).setValue(digit)
    }
    await flushPromises()

    expect(loginWithTwoFactor).toHaveBeenCalledWith('p', '123456')
    expect(push).toHaveBeenCalledTimes(1)
  })

  it('shows the refusal, clears the code and stays on the step when the code is wrong', async () => {
    const { push } = stubAuthRuntime()
    login.mockResolvedValue({ twoFactorRequired: true, pendingToken: 'p' })
    loginWithTwoFactor.mockRejectedValue({ data: { detail: 'Invalid code.' } })
    const wrapper = await mountAuthPage(LoginPage)
    await signIn(wrapper)

    for (const [index, digit] of ['0', '0', '0', '0', '0', '0'].entries()) {
      await wrapper.find(`[data-testid="otp-input-${index}"]`).setValue(digit)
    }
    await flushPromises()

    expect(wrapper.find('[data-testid="login-error"]').text()).toBe('Invalid code.')
    expect(wrapper.find('[data-testid="login-2fa-form"]').exists()).toBe(true)
    expect((wrapper.find('[data-testid="otp-input-0"]').element as HTMLInputElement).value).toBe('')
    expect(push).not.toHaveBeenCalled()
  })

  it('goes back to the password form from the code step', async () => {
    stubAuthRuntime()
    login.mockResolvedValue({ twoFactorRequired: true, pendingToken: 'p' })
    const wrapper = await mountAuthPage(LoginPage)
    await signIn(wrapper)

    await wrapper.find('[data-testid="login-2fa-cancel"]').trigger('click')

    expect(wrapper.find('[data-testid="login-form"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="login-2fa-form"]').exists()).toBe(false)
  })
  it('sends the login request again after a wrong code and a step back', async () => {
    stubAuthRuntime()
    login.mockResolvedValue({ twoFactorRequired: true, pendingToken: 'p' })
    loginWithTwoFactor.mockRejectedValue({ data: { detail: 'Invalid code.' } })
    const wrapper = await mountAuthPage(LoginPage)
    await signIn(wrapper)
    for (const [index, digit] of ['0', '0', '0', '0', '0', '0'].entries()) {
      await wrapper.find(`[data-testid="otp-input-${index}"]`).setValue(digit)
    }
    await flushPromises()

    await wrapper.find('[data-testid="login-2fa-cancel"]').trigger('click')
    await wrapper.find('form').trigger('submit')
    await flushPromises()

    expect(login).toHaveBeenCalledTimes(2)
  })

  it('does not leave the sign-in button disabled when the step is left while a code is in flight', async () => {
    stubAuthRuntime()
    login.mockResolvedValue({ twoFactorRequired: true, pendingToken: 'p' })
    let rejectCode: (reason: unknown) => void = () => undefined
    loginWithTwoFactor.mockReturnValue(new Promise((_resolve, reject) => {
      rejectCode = reject
    }))
    const wrapper = await mountAuthPage(LoginPage)
    await signIn(wrapper)
    for (const [index, digit] of ['0', '0', '0', '0', '0', '0'].entries()) {
      await wrapper.find(`[data-testid="otp-input-${index}"]`).setValue(digit)
    }

    await wrapper.find('[data-testid="login-2fa-cancel"]').trigger('click')
    await flushPromises()
    expect(wrapper.find('[data-testid="login-submit"]').attributes('disabled')).toBeUndefined()

    rejectCode({ data: { detail: 'Invalid code.' } })
    await flushPromises()
    expect(wrapper.find('[data-testid="login-error"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="login-submit"]').attributes('disabled')).toBeUndefined()
  })
})

describe('client login page (SSO mode)', () => {
  it('shows one button to the identity provider and no form', async () => {
    stubAuthRuntime({ isSso: true })
    const wrapper = await mountAuthPage(LoginPage)

    expect(wrapper.find('[data-testid="login-sso"]').attributes('href')).toBe('/api/portal/auth/sso/authorize')
    expect(wrapper.find('form').exists()).toBe(false)
    expect(wrapper.find('[data-testid="login-register"]').exists()).toBe(false)
  })
})

describe('client login page (demo mode)', () => {
  /** The published customer account used by these tests. */
  const CLIENT = { role: DemoAccountRole.Client, email: 'client@demo.test', password: 'demo-pass' }

  /**
   * Mounts the page with the demo store already holding the given status.
   *
   * @param enabled - Whether the deployment is a demo.
   * @returns The mounted page.
   */
  const mountWithDemo = async (enabled: boolean) => {
    const pinia = createPinia()
    setActivePinia(pinia)
    const demo = useDemoStore()
    demo.enabled = enabled
    demo.accounts = [{ role: DemoAccountRole.Admin, email: 'admin@demo.test', password: 'x' }, CLIENT]
    return mountPage(LoginPage, { pinia, components: { AppLogo: APP_LOGO_STUB } })
  }

  it('offers no demo account outside a demo', async () => {
    stubAuthRuntime()
    const wrapper = await mountWithDemo(false)

    expect(wrapper.find('[data-testid="login-demo"]').exists()).toBe(false)
  })

  it('shows the client account, never the admin one, in a demo', async () => {
    stubAuthRuntime()
    const wrapper = await mountWithDemo(true)

    expect(wrapper.find('[data-testid="login-demo"]').exists()).toBe(true)
    expect(wrapper.html()).not.toContain('admin@demo.test')
  })

  it('keeps the sign-in form visible beside the demo hint', async () => {
    stubAuthRuntime()
    const wrapper = await mountWithDemo(true)

    expect(wrapper.find('[data-testid="login-form"]').exists()).toBe(true)
  })

  it('fills the form when the demo button is pressed', async () => {
    stubAuthRuntime()
    const wrapper = await mountWithDemo(true)

    await wrapper.find('[data-testid="login-demo-use"]').trigger('click')

    expect((wrapper.find('[data-testid="login-email"]').element as HTMLInputElement).value).toBe(CLIENT.email)
    expect((wrapper.find('[data-testid="login-password"]').element as HTMLInputElement).value).toBe(CLIENT.password)
  })
})
