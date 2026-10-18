/**
 * Unit tests for UiDivider: a one-pixel rule that takes its spacing from the caller's classes.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiDivider from '~/components/ui/UiDivider.vue'

describe('UiDivider', () => {
  it('renders a horizontal rule one pixel high', () => {
    const wrapper = mountWithKit(UiDivider)

    expect(wrapper.element.tagName).toBe('HR')
    expect(wrapper.classes()).toEqual(expect.arrayContaining(['h-px', 'w-full', 'border-0']))
  })

  it('carries the caller\'s spacing class', () => {
    const wrapper = mountWithKit(UiDivider, { attrs: { class: 'my-10' } })

    expect(wrapper.classes()).toContain('my-10')
  })

  it('exposes the data-testid of the kit', () => {
    const wrapper = mountWithKit(UiDivider)

    expect(wrapper.attributes('data-testid')).toBe('ui-divider')
  })
})
