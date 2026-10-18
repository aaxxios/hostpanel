/**
 * Unit tests for UiPageHeader: the h1 title, description, icon, breadcrumbs and actions slots.
 */
import { describe, expect, it } from 'vitest'
import { Server } from 'lucide-vue-next'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiPageHeader from '~/components/ui/UiPageHeader.vue'

describe('UiPageHeader', () => {
  it('renders the title as the page h1', () => {
    const wrapper = mountWithKit(UiPageHeader, { props: { title: 'Services' } })

    expect(wrapper.find('h1').text()).toBe('Services')
    expect(wrapper.element.tagName).toBe('HEADER')
  })

  it('renders the description and the icon when given', () => {
    const wrapper = mountWithKit(UiPageHeader, { props: { title: 'Services', description: 'Everything you host.', icon: Server } })

    expect(wrapper.text()).toContain('Everything you host.')
    expect(wrapper.find('svg').exists()).toBe(true)
  })

  it('renders neither without them', () => {
    const wrapper = mountWithKit(UiPageHeader, { props: { title: 'Services' } })

    expect(wrapper.find('p').exists()).toBe(false)
    expect(wrapper.find('svg').exists()).toBe(false)
  })

  it('renders the breadcrumbs and actions slots', () => {
    const wrapper = mountWithKit(UiPageHeader, {
      props: { title: 'Services' },
      slots: { breadcrumbs: 'Home / Services', actions: 'Order new' },
    })

    expect(wrapper.text()).toContain('Home / Services')
    expect(wrapper.text()).toContain('Order new')
  })

  it('omits the empty slot wrappers', () => {
    const wrapper = mountWithKit(UiPageHeader, { props: { title: 'Services' } })

    expect(wrapper.html()).not.toContain('flex-wrap')
    expect(wrapper.html()).not.toContain('mb-4')
  })
})
