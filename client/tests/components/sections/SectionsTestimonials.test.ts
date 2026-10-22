/**
 * Unit tests for SectionsTestimonials: one slide per testimonial read from the storefront store
 * with its author, the readable light-mode card, and the section hiding itself when there are
 * none. The carousel library is stood in for.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { testimonialFixture } from '~/tests/helpers/storefrontFixtures'
import SectionsTestimonials from '~/components/sections/SectionsTestimonials.vue'
import { CLASSIC_COMPONENTS } from '~/tests/helpers/classicKit'

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

/** The mocked storefront endpoints, re-created for every test. */
const api = { fetchPartners: vi.fn(), fetchTestimonials: vi.fn(), fetchProjects: vi.fn(), fetchFaqs: vi.fn() }

vi.mock('~/composables/apis/useStorefrontApi', () => ({
  /**
   * Stands in for the storefront API composable so the store runs without transport.
   *
   * @returns The mocked endpoints.
   */
  useStorefrontApi: () => api,
}))

/**
 * Mounts the section and lets the testimonials load.
 *
 * @returns The wrapper.
 */
const mountSection = async () => {
  const wrapper = mountWithKit(SectionsTestimonials, { components: CLASSIC_COMPONENTS })
  await flushPromises()
  return wrapper
}

describe('SectionsTestimonials', () => {
  beforeEach(() => {
    api.fetchTestimonials.mockReset()
    api.fetchTestimonials.mockResolvedValue([
      testimonialFixture(1),
      testimonialFixture(2, { role: null, company: null, rating: 3 }),
    ])
  })

  it('carries every quote in the server-render fallback, where the browser-only carousel is absent', async () => {
    const wrapper = mountWithKit(SectionsTestimonials, {
      components: CLASSIC_COMPONENTS,
      stubs: { ClientOnly: { template: '<div><slot name="fallback" /></div>' } },
    })
    await flushPromises()

    const items = wrapper.find('[data-testid="testimonials-fallback"]').findAll('li')
    expect(items).toHaveLength(2)
    expect(wrapper.find('[data-testid="swiper"]').exists()).toBe(false)
  })

  it('renders a slide per testimonial with its quote, author, role and company', async () => {
    const wrapper = await mountSection()

    const slide = wrapper.find('[data-testid="testimonial-1"]')
    expect(wrapper.findAll('[data-testid^="testimonial-"]')).toHaveLength(2)
    expect(slide.text()).toContain('Example quote 1.')
    expect(slide.text()).toContain('Example Person 1')
    expect(slide.text()).toContain('Founder')
    expect(slide.text()).toContain('Example Co')
  })

  it('omits the role and company lines of a testimonial that has neither', async () => {
    const wrapper = await mountSection()

    const text = wrapper.find('[data-testid="testimonial-2"]').text()
    expect(text).toContain('Example Person 2')
    expect(text).not.toContain('Founder')
    expect(text).not.toContain('Example Co')
  })

  it('shows the author initials in the avatar', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="testimonial-1"]').text()).toContain('EP')
  })

  it('draws as many stars as the rating', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="testimonial-1"]').findAll('svg.text-classic-brand')).toHaveLength(5)
    expect(wrapper.find('[data-testid="testimonial-2"]').findAll('svg.text-classic-brand')).toHaveLength(3)
  })

  it('draws the quote on a flat classic card with no gradient', async () => {
    const slide = (await mountSection()).find('[data-testid="testimonial-1"]')

    expect(slide.classes()).toContain('bg-classic-surface')
    expect(slide.classes().some((name) => name.includes('gradient') || name.startsWith('dark:from-'))).toBe(false)
  })

  it('draws no oversized decorative quote glyph', async () => {
    const slide = (await mountSection()).find('[data-testid="testimonial-1"]')

    expect(slide.find('svg[width="64"]').exists()).toBe(false)
  })

  it('shows the swipe hint', async () => {
    expect((await mountSection()).text()).toContain('testimonials.hint')
  })

  it('renders nothing when there are no testimonials', async () => {
    api.fetchTestimonials.mockResolvedValue([])

    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="sections-testimonials"]').exists()).toBe(false)
  })
})
