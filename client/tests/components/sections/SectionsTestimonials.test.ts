/**
 * Unit tests for SectionsTestimonials: one slide per testimonial with its author, the readable
 * light-mode card, and the row of client figures. The carousel library is stood in for.
 */
import { describe, expect, it, vi } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import SectionsTestimonials from '~/components/sections/SectionsTestimonials.vue'
import MarketingSectionHeader from '~/components/marketing/MarketingSectionHeader.vue'
import { testimonials } from '~/lib/data'

vi.mock('swiper/vue', async () => {
  const { defineComponent, h } = await import('vue')
  return {
    /** Stand-in carousel: renders its slides in a plain element. */
    Swiper: defineComponent({ name: 'Swiper', setup: (_props, { slots }) => () => h('div', { 'data-testid': 'swiper' }, slots.default?.()) }),
    /** Stand-in slide. */
    SwiperSlide: defineComponent({ name: 'SwiperSlide', setup: (_props, { slots }) => () => h('div', slots.default?.()) }),
  }
})
vi.mock('swiper/modules', () => ({ Autoplay: {}, Pagination: {} }))
vi.mock('swiper/css', () => ({}))
vi.mock('swiper/css/pagination', () => ({}))

/** Mounts the section. */
const mountSection = () => mountWithKit(SectionsTestimonials, { components: { MarketingSectionHeader } })

describe('SectionsTestimonials', () => {
  it('renders a slide per testimonial with its quote, author and company', () => {
    const wrapper = mountSection()

    const first = testimonials[0]
    const slide = wrapper.find(`[data-testid="testimonial-${first?.id}"]`)
    expect(wrapper.findAll('[data-testid^="testimonial-testimonial"]')).toHaveLength(testimonials.length)
    expect(slide.text()).toContain(`testimonials.items.${first?.id}.text`)
    expect(slide.text()).toContain(first?.name)
    expect(slide.text()).toContain(first?.company)
  })

  it('shows the author initials in the avatar', () => {
    const wrapper = mountSection()

    expect(wrapper.find(`[data-testid="testimonial-${testimonials[0]?.id}"]`).text()).toContain('SJ')
  })

  it('puts the gradient on dark mode only, so the quote stays readable on the light surface', () => {
    const slide = mountSection().find(`[data-testid="testimonial-${testimonials[0]?.id}"]`)

    expect(slide.classes().some((name) => name.startsWith('dark:from-'))).toBe(true)
    expect(slide.classes().some((name) => name === 'bg-gradient-to-br')).toBe(false)
  })

  it('shows the four client figures and the swipe hint', () => {
    const wrapper = mountSection()

    expect(wrapper.find('[data-testid="testimonial-stat-0"]').text()).toContain('98%')
    expect(wrapper.find('[data-testid="testimonial-stat-3"]').text()).toContain('4.9')
    expect(wrapper.text()).toContain('testimonials.hint')
  })
})
