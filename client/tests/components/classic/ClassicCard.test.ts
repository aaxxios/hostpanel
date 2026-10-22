/**
 * Unit tests for ClassicCard: the flat classic surface, its hover option and its padding.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import ClassicCard from '~/components/classic/ClassicCard.vue'

describe('ClassicCard', () => {
  it('renders its slot on the classic surface with a hairline border and a modest radius', () => {
    const wrapper = mountWithKit(ClassicCard, { slots: { default: 'Body text' } })

    expect(wrapper.text()).toContain('Body text')
    expect(wrapper.classes()).toEqual(expect.arrayContaining(['bg-classic-surface', 'border-classic-border', '!rounded-lg', 'shadow-sm']))
  })

  it('adds the hover lift only when asked', () => {
    const plain = mountWithKit(ClassicCard)
    const hover = mountWithKit(ClassicCard, { props: { hover: true } })

    expect(plain.classes()).not.toContain('hover:shadow-md')
    expect(hover.classes()).toContain('hover:shadow-md')
  })

  it('uses no gradient and no glow', () => {
    expect(mountWithKit(ClassicCard, { props: { hover: true } }).html()).not.toMatch(/gradient|blur-/)
  })
})
