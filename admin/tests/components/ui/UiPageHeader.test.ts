/**
 * Unit tests for UiPageHeader: title as the page heading, subtitle and slots.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import UiPageHeader from '@/components/ui/UiPageHeader.vue'

describe('UiPageHeader', () => {
  it('renders the title as the level-1 heading', () => {
    const wrapper = mount(UiPageHeader, { props: { title: 'Clients' } })

    expect(wrapper.get('h1').text()).toBe('Clients')
  })

  it('renders the subtitle when given', () => {
    const wrapper = mount(UiPageHeader, { props: { title: 'Clients', subtitle: 'All accounts' } })

    expect(wrapper.text()).toContain('All accounts')
  })

  it('renders the actions slot and the default slot', () => {
    const wrapper = mount(UiPageHeader, {
      props: { title: 'Clients' },
      slots: { default: '<span data-testid="extra" />', actions: '<button data-testid="act" />' },
    })

    expect(wrapper.find('[data-testid="extra"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="act"]').exists()).toBe(true)
  })
})
