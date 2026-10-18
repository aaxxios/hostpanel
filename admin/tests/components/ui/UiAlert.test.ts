/**
 * Unit tests for UiAlert: tone colours, the legacy error alias and live-region roles.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import UiAlert from '@/components/ui/UiAlert.vue'

describe('UiAlert', () => {
  it('renders the message', () => {
    const wrapper = mount(UiAlert, { slots: { default: 'Server said no' } })

    expect(wrapper.text()).toBe('Server said no')
  })

  it('is a warning by default', () => {
    const wrapper = mount(UiAlert)

    expect(wrapper.classes()).toContain('text-status-yellow')
    expect(wrapper.attributes('role')).toBe('alert')
  })

  it('treats error as the danger tone', () => {
    const wrapper = mount(UiAlert, { props: { variant: 'error' } })

    expect(wrapper.classes()).toContain('text-status-red')
    expect(wrapper.attributes('role')).toBe('alert')
  })

  it('announces info and success politely', () => {
    expect(mount(UiAlert, { props: { variant: 'info' } }).attributes('role')).toBe('status')
    expect(mount(UiAlert, { props: { variant: 'success' } }).attributes('role')).toBe('status')
  })

  it('keeps data-testid', () => {
    expect(mount(UiAlert, { attrs: { 'data-testid': 'probe' } }).attributes('data-testid')).toBe('probe')
  })
})
