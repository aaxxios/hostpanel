/**
 * Unit tests for UiBackdrop: it is decorative, and each variant draws its own decoration.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiBackdrop from '~/components/ui/UiBackdrop.vue'
import { UiBackdropVariant } from '~/utils/ui/enums'

describe('UiBackdrop', () => {
  it('is hidden from assistive technology and ignores the pointer', () => {
    const wrapper = mountWithKit(UiBackdrop)

    expect(wrapper.attributes('aria-hidden')).toBe('true')
    expect(wrapper.classes()).toEqual(expect.arrayContaining(['pointer-events-none', 'absolute', 'inset-0']))
  })

  it('draws the aurora glows by default', () => {
    const wrapper = mountWithKit(UiBackdrop)

    expect(wrapper.attributes('data-variant')).toBe('aurora')
    expect(wrapper.html()).toContain('bg-glow1')
    expect(wrapper.html()).toContain('bg-glow2')
  })

  it('draws the nova wash', () => {
    const wrapper = mountWithKit(UiBackdrop, { props: { variant: UiBackdropVariant.Nova } })

    expect(wrapper.classes()).toContain('bg-gradient-to-b')
    expect(wrapper.html()).not.toContain('bg-glow1')
  })

  it('draws grid lines', () => {
    const wrapper = mountWithKit(UiBackdrop, { props: { variant: UiBackdropVariant.Grid } })

    expect(wrapper.html()).toContain('pattern-grid-50')
  })

  it('draws a dot matrix', () => {
    const wrapper = mountWithKit(UiBackdrop, { props: { variant: UiBackdropVariant.Dots } })

    expect(wrapper.html()).toContain('pattern-dots-30')
  })
})
