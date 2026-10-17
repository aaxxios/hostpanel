/**
 * Unit tests for UiStack: direction, gap, alignment and wrapping classes.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import UiStack from '@/components/ui/UiStack.vue'

describe('UiStack', () => {
  it('stacks vertically with a medium gap by default', () => {
    const wrapper = mount(UiStack, { slots: { default: 'x' } })

    expect(wrapper.classes()).toEqual(expect.arrayContaining(['flex', 'flex-col', 'gap-4']))
  })

  it('lays out a row with the chosen gap, alignment and distribution', () => {
    const wrapper = mount(UiStack, {
      props: { direction: 'row', gap: 'sm', align: 'center', justify: 'between', wrap: true },
    })

    expect(wrapper.classes()).toEqual(expect.arrayContaining([
      'flex-row', 'gap-2', 'items-center', 'justify-between', 'flex-wrap',
    ]))
  })

  it('passes attributes through', () => {
    expect(mount(UiStack, { attrs: { 'data-testid': 'probe' } }).attributes('data-testid')).toBe('probe')
  })
})
