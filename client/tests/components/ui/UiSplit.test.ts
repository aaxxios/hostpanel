/**
 * Unit tests for UiSplit: the two slots, the width ratios, the gap and the reversed order.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiSplit from '~/components/ui/UiSplit.vue'
import { UiGap, UiSplitRatio } from '~/utils/ui/enums'

describe('UiSplit', () => {
  it('renders the start and end slots in order', () => {
    const wrapper = mountWithKit(UiSplit, { slots: { start: 'Main', end: 'Side' } })

    expect(wrapper.find('[data-testid="ui-split-start"]').text()).toBe('Main')
    expect(wrapper.find('[data-testid="ui-split-end"]').text()).toBe('Side')
  })

  it('splits evenly by default', () => {
    const wrapper = mountWithKit(UiSplit)

    expect(wrapper.classes()).toEqual(expect.arrayContaining(['grid', 'lg:grid-cols-2', 'gap-6']))
    expect(wrapper.find('[data-testid="ui-split-start"]').classes()).not.toContain('lg:col-span-2')
    expect(wrapper.find('[data-testid="ui-split-end"]').classes()).not.toContain('lg:col-span-2')
  })

  it('gives the start half two thirds', () => {
    const wrapper = mountWithKit(UiSplit, { props: { ratio: UiSplitRatio.StartWide } })

    expect(wrapper.classes()).toContain('lg:grid-cols-3')
    expect(wrapper.find('[data-testid="ui-split-start"]').classes()).toContain('lg:col-span-2')
    expect(wrapper.find('[data-testid="ui-split-end"]').classes()).not.toContain('lg:col-span-2')
  })

  it('gives the end half two thirds', () => {
    const wrapper = mountWithKit(UiSplit, { props: { ratio: UiSplitRatio.EndWide } })

    expect(wrapper.find('[data-testid="ui-split-end"]').classes()).toContain('lg:col-span-2')
    expect(wrapper.find('[data-testid="ui-split-start"]').classes()).not.toContain('lg:col-span-2')
  })

  it('swaps the visual order from lg when reversed, keeping the DOM order', () => {
    const wrapper = mountWithKit(UiSplit, { props: { reverse: true }, slots: { start: 'Main', end: 'Side' } })

    expect(wrapper.find('[data-testid="ui-split-start"]').classes()).toContain('lg:order-2')
    expect(wrapper.find('[data-testid="ui-split-end"]').classes()).toContain('lg:order-1')
    expect(wrapper.findAll('[data-testid^="ui-split-"]')[0]?.text()).toBe('Main')
  })

  it('maps the gap', () => {
    expect(mountWithKit(UiSplit, { props: { gap: UiGap.Xl } }).classes()).toContain('gap-10')
  })
})
