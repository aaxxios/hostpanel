/**
 * Unit tests for UiText: native paragraph, tone and size from tokens.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import UiText from '@/components/ui/UiText.vue'

describe('UiText', () => {
  it('renders the native p with its slot', () => {
    const wrapper = mount(UiText, { slots: { default: 'content' } })

    expect(wrapper.element.tagName).toBe('P')
    expect(wrapper.text()).toBe('content')
  })

  it('passes classes and data attributes through', () => {
    const wrapper = mount(UiText, { attrs: { class: 'x-class', 'data-testid': 'probe' } })

    expect(wrapper.classes()).toContain('x-class')
    expect(wrapper.attributes('data-testid')).toBe('probe')
  })

  it('adds no typography in the plain default', () => {
    expect(mount(UiText).classes()).toEqual([])
  })

  it('colours and sizes from the named props', () => {
    const wrapper = mount(UiText, { props: { tone: 'danger', size: 'sm' } })

    expect(wrapper.classes()).toEqual(expect.arrayContaining(['text-status-red', 'text-[0.78rem]']))
  })
})
