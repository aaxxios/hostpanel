/**
 * Unit tests for AuthShell: the brand, the title, the optional subtitle and the slot.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import AuthShell from '@/components/auth/AuthShell.vue'
import { createTestI18n } from '../../helpers/i18n'

describe('AuthShell', () => {
  it('renders the title, the subtitle and the slot content', () => {
    const wrapper = mount(AuthShell, {
      props: { title: 'Admin Panel', subtitle: 'Sign in please' },
      slots: { default: '<p data-testid="inner">Body</p>' },
      global: { plugins: [createTestI18n()] },
    })

    expect(wrapper.get('[data-testid="auth-title"]').text()).toBe('Admin Panel')
    expect(wrapper.text()).toContain('Sign in please')
    expect(wrapper.get('[data-testid="inner"]').text()).toBe('Body')
    expect(wrapper.text()).toContain('Innovayse')
  })

  it('omits the subtitle when none is given', () => {
    const wrapper = mount(AuthShell, {
      props: { title: 'Only title' },
      global: { plugins: [createTestI18n()] },
    })

    expect(wrapper.text()).not.toContain('Sign in please')
  })
})
