/**
 * Tests for SettingsSection: the titled card that groups the controls of a settings screen.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import SettingsSection from '@/components/settings/SettingsSection.vue'

describe('SettingsSection', () => {
  it('renders the title as a heading and the slot as the body', () => {
    const wrapper = mount(SettingsSection, {
      props: { title: 'Branding' },
      slots: { default: '<p data-testid="body">Controls</p>' },
    })

    expect(wrapper.get('h2').text()).toBe('Branding')
    expect(wrapper.get('[data-testid="body"]').text()).toBe('Controls')
  })

  it('renders the description only when one is given', () => {
    const without = mount(SettingsSection, { props: { title: 'A' } })
    const withText = mount(SettingsSection, { props: { title: 'A', description: 'What this does' } })

    expect(without.find('p').exists()).toBe(false)
    expect(withText.get('p').text()).toBe('What this does')
  })

  it('keeps the classes the caller passes on the card', () => {
    const wrapper = mount(SettingsSection, { props: { title: 'A' }, attrs: { class: 'mb-24' } })

    expect(wrapper.classes()).toContain('mb-24')
  })
})
