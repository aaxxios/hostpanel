/**
 * Mount tests for LoginView: the mode decides what is drawn, a refusal shows the API's own
 * sentence, and a failed mode lookup offers a retry.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import LoginView from '@/views/LoginView.vue'
import { useDemoStore } from '@/stores/demo'
import { DemoAccountRole } from '@/types/demo'
import { mountRouted } from '../helpers/mount'
import { stubRoutes } from '../helpers/routes'
import { json, problem } from '../helpers/fetch'

/**
 * Mounts the login screen and waits for the mode lookup.
 *
 * @returns The wrapper.
 */
const mountLogin = async () => {
  const { wrapper } = await mountRouted(LoginView, '/login', '/login')
  await flushPromises()
  return wrapper
}

describe('LoginView', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('draws the email and password form in local mode', async () => {
    stubRoutes({
      'GET /api/auth/mode': () => json({ mode: 'local' }),
      'GET /api/auth/setup-required': () => json({ required: false }),
    })
    const wrapper = await mountLogin()

    expect(wrapper.find('[data-testid="login-email"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="login-password"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="login-submit"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="login-sso"]').exists()).toBe(false)
  })

  it('draws the single SSO button in SSO mode', async () => {
    stubRoutes({
      'GET /api/auth/mode': () => json({ mode: 'sso' }),
      'GET /api/auth/setup-required': () => json({ required: false }),
    })
    const wrapper = await mountLogin()

    expect(wrapper.find('[data-testid="login-sso"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="login-email"]').exists()).toBe(false)
  })

  it('shows the sentence the API returned when the sign-in is refused', async () => {
    stubRoutes({
      'GET /api/auth/mode': () => json({ mode: 'local' }),
      'GET /api/auth/setup-required': () => json({ required: false }),
      'POST /api/auth/login': () => problem('Wrong email or password.', 401),
    })
    const wrapper = await mountLogin()

    await wrapper.get('[data-testid="login-email"]').setValue('a@b.com')
    await wrapper.get('[data-testid="login-password"]').setValue('nope')
    await wrapper.get('form').trigger('submit')
    await flushPromises()

    expect(wrapper.get('[data-testid="login-error"]').text()).toContain('Wrong email or password.')
  })

  it('offers the first-account shortcut while setup is outstanding', async () => {
    stubRoutes({
      'GET /api/auth/mode': () => json({ mode: 'local' }),
      'GET /api/auth/setup-required': () => json({ required: true }),
    })
    const wrapper = await mountLogin()

    expect(wrapper.find('[data-testid="login-setup-notice"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="login-go-setup"]').exists()).toBe(true)
  })

  it('offers a retry when the mode lookup failed', async () => {
    stubRoutes({
      'GET /api/auth/mode': () => problem('Unavailable.', 503),
      'GET /api/auth/setup-required': () => problem('Unavailable.', 503),
    })
    const wrapper = await mountLogin()

    expect(wrapper.find('[data-testid="login-retry"]').exists()).toBe(true)
    expect(wrapper.get('[data-testid="login-error"]').text()).toContain('Unavailable.')
  })
})

describe('LoginView demo account', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  /**
   * Mounts the login screen in local mode with the demo store filled in.
   *
   * @param enabled - Whether demo mode is on.
   * @returns The wrapper.
   */
  const mountDemoLogin = async (enabled: boolean) => {
    stubRoutes({
      'GET /api/auth/mode': () => json({ mode: 'local' }),
      'GET /api/auth/setup-required': () => json({ required: false }),
    })
    const wrapper = await mountLogin()
    const demo = useDemoStore()
    demo.enabled = enabled
    demo.accounts = [{ role: DemoAccountRole.Admin, email: 'demo@x.test', password: 'demo-pw' }]
    await flushPromises()
    return wrapper
  }

  it('fills the form from the demo Admin account', async () => {
    const wrapper = await mountDemoLogin(true)

    await wrapper.get('[data-testid="demo-login-use"]').trigger('click')

    expect((wrapper.get('[data-testid="login-email"]').element as HTMLInputElement).value).toBe('demo@x.test')
    expect((wrapper.get('[data-testid="login-password"]').element as HTMLInputElement).value).toBe('demo-pw')
  })

  it('offers no demo account outside demo mode', async () => {
    const wrapper = await mountDemoLogin(false)

    expect(wrapper.find('[data-testid="demo-login"]').exists()).toBe(false)
  })
})
