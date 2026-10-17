/**
 * Unit tests for UiNavItem: the element it renders, the active, disabled and warning looks, and
 * the loading state.
 */
import { describe, expect, it } from 'vitest'
import { Zap } from 'lucide-vue-next'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiNavItem from '~/components/ui/UiNavItem.vue'
import { UiNavTone } from '~/utils/ui/enums'

describe('UiNavItem', () => {
  it('renders a button by default and emits click', async () => {
    const wrapper = mountWithKit(UiNavItem, { slots: { default: 'Overview' }, attrs: { onClick: () => undefined } })

    expect(wrapper.element.tagName).toBe('BUTTON')
    expect(wrapper.attributes('type')).toBe('button')
    expect(wrapper.text()).toBe('Overview')
  })

  it('renders a route link for `to`', () => {
    const wrapper = mountWithKit(UiNavItem, { props: { to: '/client/tickets/new' }, slots: { default: 'Ticket' } })

    expect(wrapper.element.tagName).toBe('A')
    expect(wrapper.attributes('href')).toBe('/client/tickets/new')
  })

  it('renders an anchor that opens a new tab for `href`', () => {
    const wrapper = mountWithKit(UiNavItem, { props: { href: 'https://host.example:2096' }, slots: { default: 'Webmail' } })

    expect(wrapper.attributes('target')).toBe('_blank')
    expect(wrapper.attributes('rel')).toBe('noopener noreferrer')
  })

  it('highlights the active entry', () => {
    const wrapper = mountWithKit(UiNavItem, { props: { active: true } })

    expect(wrapper.classes()).toContain('font-medium')
    expect(wrapper.classes()).toContain('text-primary-600')
  })

  it('greys out and disables a disabled entry', () => {
    const wrapper = mountWithKit(UiNavItem, { props: { disabled: true } })

    expect(wrapper.attributes('disabled')).toBeDefined()
    expect(wrapper.classes()).toContain('cursor-not-allowed')
  })

  it('draws the warning tone in yellow', () => {
    const wrapper = mountWithKit(UiNavItem, { props: { tone: UiNavTone.Warning } })

    expect(wrapper.classes()).toContain('text-yellow-600')
  })

  it('swaps the icon for a spinner while loading and disables the button', () => {
    const wrapper = mountWithKit(UiNavItem, { props: { icon: Zap, loading: true } })

    expect(wrapper.find('svg').classes()).toContain('animate-spin')
    expect(wrapper.attributes('disabled')).toBeDefined()
  })

  it('passes the test id to the element', () => {
    const wrapper = mountWithKit(UiNavItem, { props: { testid: 'nav-overview' } })

    expect(wrapper.attributes('data-testid')).toBe('nav-overview')
  })
})
