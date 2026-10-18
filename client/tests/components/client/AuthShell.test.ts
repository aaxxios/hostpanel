/**
 * Unit tests for ClientAuthShell: the frame around every signed-out client page.
 *
 * @module tests/components/client/AuthShell.test
 */

import { describe, expect, it } from 'vitest'
import { mountWithLogo } from '~/tests/helpers/authScreens'
import AuthShell from '~/components/client/AuthShell.vue'

describe('ClientAuthShell', () => {
  it('renders the title, the subtitle and the slot inside the card', () => {
    const wrapper = mountWithLogo(AuthShell, {
      props: { title: 'Sign in', subtitle: 'Welcome back' },
      slots: { default: 'card body' }
    })

    expect(wrapper.find('[data-testid="auth-shell-title"]').text()).toBe('Sign in')
    expect(wrapper.find('[data-testid="auth-shell-subtitle"]').text()).toBe('Welcome back')
    expect(wrapper.text()).toContain('card body')
  })

  it('draws the aurora backdrop', () => {
    const wrapper = mountWithLogo(AuthShell)

    expect(wrapper.find('[data-testid="ui-backdrop"]').attributes('data-variant')).toBe('aurora')
  })

  it('shows the logo by default and hides it on request', () => {
    expect(mountWithLogo(AuthShell).find('[data-testid="auth-logo"]').exists()).toBe(true)
    expect(mountWithLogo(AuthShell, { props: { showLogo: false } }).find('[data-testid="auth-logo"]').exists()).toBe(false)
  })

  it('omits the heading when there is no title', () => {
    const wrapper = mountWithLogo(AuthShell)

    expect(wrapper.find('[data-testid="auth-shell-title"]').exists()).toBe(false)
  })

  it('renders the footer slot only when one is given', () => {
    const without = mountWithLogo(AuthShell)
    const withFooter = mountWithLogo(AuthShell, { slots: { footer: 'no account?' } })

    expect(without.text()).not.toContain('no account?')
    expect(withFooter.text()).toContain('no account?')
  })
})
