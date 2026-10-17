/**
 * Unit tests for UiBadge: tone colours, dot and compact size.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import UiBadge from '@/components/ui/UiBadge.vue'

describe('UiBadge', () => {
  it('renders its slot in a pill', () => {
    const wrapper = mount(UiBadge, { slots: { default: 'Active' } })

    expect(wrapper.text()).toBe('Active')
    expect(wrapper.classes()).toContain('rounded-full')
  })

  it('takes its colours from the tone', () => {
    const wrapper = mount(UiBadge, { props: { tone: 'success' } })

    expect(wrapper.classes()).toEqual(expect.arrayContaining(['text-status-green', 'border-status-green/20']))
  })

  it('draws a status dot on request', () => {
    const plain = mount(UiBadge)
    const dotted = mount(UiBadge, { props: { dot: true, tone: 'danger' } })

    expect(plain.element.children).toHaveLength(0)
    expect(dotted.element.children).toHaveLength(1)
    expect(dotted.element.children[0]?.className).toContain('bg-status-red')
  })

  it('shrinks in the small size', () => {
    expect(mount(UiBadge, { props: { small: true } }).classes()).toContain('text-[0.65rem]')
  })
})
