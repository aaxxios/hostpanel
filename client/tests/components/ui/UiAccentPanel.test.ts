/**
 * Unit tests for UiAccentPanel: neutral by default, accent frame and enlargement when highlighted.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiAccentPanel from '~/components/ui/UiAccentPanel.vue'

describe('UiAccentPanel', () => {
  it('is a neutral framed panel by default and renders its slot', () => {
    const wrapper = mountWithKit(UiAccentPanel, { slots: { default: 'plan' } })

    expect(wrapper.text()).toBe('plan')
    expect(wrapper.classes()).toContain('border-gray-200')
    expect(wrapper.classes()).not.toContain('md:scale-105')
  })

  it('takes the accent frame and grows when highlighted', () => {
    const wrapper = mountWithKit(UiAccentPanel, { props: { highlighted: true } })

    expect(wrapper.classes()).toEqual(expect.arrayContaining(['md:scale-105', 'border-[color:var(--accent)]']))
    expect(wrapper.classes()).not.toContain('border-gray-200')
  })
})
