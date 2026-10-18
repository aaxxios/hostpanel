/**
 * Unit tests for UiGlassPanel: it renders its slot on a frosted dark panel and keeps the caller's classes.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiGlassPanel from '~/components/ui/UiGlassPanel.vue'

describe('UiGlassPanel', () => {
  it('renders its slot on a blurred, translucent panel', () => {
    const wrapper = mountWithKit(UiGlassPanel, { slots: { default: 'copy' } })

    expect(wrapper.text()).toBe('copy')
    expect(wrapper.classes()).toEqual(expect.arrayContaining(['bg-black/30', 'backdrop-blur-xl']))
  })

  it('keeps the classes it is given', () => {
    const wrapper = mountWithKit(UiGlassPanel, { attrs: { class: 'max-w-4xl text-center' } })

    expect(wrapper.classes()).toEqual(expect.arrayContaining(['max-w-4xl', 'text-center']))
  })
})
