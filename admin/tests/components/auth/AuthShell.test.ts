/**
 * Unit tests for AuthShell: the brand, the title, the optional subtitle and the slot.
 */
import { beforeEach, describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import AuthShell from '@/components/auth/AuthShell.vue'
import { createTestI18n } from '../../helpers/i18n'

/**
 * Mounts the shell with the given props.
 *
 * @param props - Title and optional subtitle.
 * @returns The wrapper.
 */
const mountShell = (props: { title: string, subtitle?: string }) => mount(AuthShell, {
  props,
  slots: { default: '<p data-testid="inner">Body</p>' },
  global: { plugins: [createTestI18n()] },
})

describe('AuthShell', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('renders the title, the subtitle and the slot content', () => {
    const wrapper = mountShell({ title: 'Admin Panel', subtitle: 'Sign in please' })

    expect(wrapper.get('[data-testid="auth-title"]').text()).toBe('Admin Panel')
    expect(wrapper.text()).toContain('Sign in please')
    expect(wrapper.get('[data-testid="inner"]').text()).toBe('Body')
  })

  it('shows the built-in wordmark for each theme, named for screen readers', () => {
    const wrapper = mountShell({ title: 'Only title' })

    expect(wrapper.get('[data-testid="brand-logo-dark"]').attributes('src')).toBe('/logo-dark.svg')
    expect(wrapper.get('[data-testid="brand-logo-light"]').attributes('src')).toBe('/logo.svg')
    expect(wrapper.get('[data-testid="brand-logo-dark"]').attributes('alt')).toBe('Hostpanel')
  })

  it('omits the subtitle when none is given', () => {
    const wrapper = mountShell({ title: 'Only title' })

    expect(wrapper.text()).not.toContain('Sign in please')
  })
})
