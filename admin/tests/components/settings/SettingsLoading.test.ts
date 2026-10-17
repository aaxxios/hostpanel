/**
 * Tests for SettingsLoading: the caption and a polite, busy status region.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import SettingsLoading from '@/components/settings/SettingsLoading.vue'

describe('SettingsLoading', () => {
  it('shows the caption in a busy status region', () => {
    const wrapper = mount(SettingsLoading, { props: { message: 'Loading slides...' } })

    expect(wrapper.text()).toContain('Loading slides...')
    expect(wrapper.attributes('role')).toBe('status')
    expect(wrapper.attributes('aria-busy')).toBe('true')
  })

  it('draws a skeleton under the caption', () => {
    const wrapper = mount(SettingsLoading, { props: { message: 'x' } })

    expect(wrapper.find('.animate-pulse').exists()).toBe(true)
  })
})
