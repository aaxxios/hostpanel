/**
 * Unit tests for UiCarouselDots: one labelled button per slide, `aria-current` and the pill only on the
 * active dot, one colour logic for the rest, and `select` carrying the pressed dot's index.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiCarouselDots from '~/components/ui/UiCarouselDots.vue'

/**
 * Mounts the dots.
 *
 * @param props Count and active index.
 * @returns The wrapper.
 */
const mountDots = (props: { count: number, active: number }) => mountWithKit(UiCarouselDots, { props })

describe('UiCarouselDots', () => {
  it('renders one labelled button per slide', () => {
    const wrapper = mountDots({ count: 3, active: 0 })

    const dots = wrapper.findAll('button')
    expect(dots).toHaveLength(3)
    for (const dot of dots) {
      expect(dot.attributes('type')).toBe('button')
      expect(dot.attributes('aria-label')).toBe('common.goToSlide')
    }
  })

  it('marks only the active dot with aria-current and the elongated brand pill', () => {
    const wrapper = mountDots({ count: 3, active: 1 })

    const dots = wrapper.findAll('button')
    expect(dots.map((dot) => dot.attributes('aria-current'))).toEqual([undefined, 'true', undefined])
    expect(dots[1]!.classes()).toEqual(expect.arrayContaining(['w-6', 'bg-classic-fill']))
    expect(dots[0]!.classes()).toEqual(expect.arrayContaining(['w-2', 'bg-classic-muted/45', 'hover:bg-classic-muted/80']))
  })

  it('gives every dot the same inactive look: there is no visited colour', () => {
    const wrapper = mountDots({ count: 4, active: 2 })

    const inactive = wrapper.findAll('button').filter((_dot, index) => index !== 2)
    expect(new Set(inactive.map((dot) => dot.classes().join(' '))).size).toBe(1)
  })

  it('has a focus-visible ring and respects reduced motion', () => {
    const classes = mountDots({ count: 2, active: 0 }).find('button').classes().join(' ')

    expect(classes).toContain('focus-visible:ring-2')
    expect(classes).toContain('motion-reduce:transition-none')
  })

  it('emits select with the zero-based index of the pressed dot', async () => {
    const wrapper = mountDots({ count: 3, active: 0 })

    await wrapper.find('[data-testid="carousel-dot-2"]').trigger('click')

    expect(wrapper.emitted('select')).toEqual([[2]])
  })
})
