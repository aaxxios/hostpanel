/**
 * Unit tests for ProductsFullSlider: a slide per catalogue slide in its own brand colour, the
 * calls to action, the arrows that only show with several slides, and the scroll-down button. The
 * carousel library is stood in for.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountPage, stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import ProductsFullSlider from '~/components/sections/SectionsProductsFullSlider.vue'
import type { SlidePublic } from '~/types/slidepublic'

vi.mock('swiper/vue', async () => {
  const { defineComponent, h } = await import('vue')
  return {
    /** Stand-in carousel: renders its slides in a plain element. */
    Swiper: defineComponent({ name: 'Swiper', setup: (_props, { slots }) => () => h('div', { 'data-testid': 'swiper' }, slots.default?.()) }),
    /** Stand-in slide. */
    SwiperSlide: defineComponent({ name: 'SwiperSlide', setup: (_props, { slots }) => () => h('div', slots.default?.()) }),
  }
})
vi.mock('swiper/modules', () => ({ Autoplay: {}, Navigation: {}, Pagination: {} }))
vi.mock('swiper/css', () => ({}))
vi.mock('swiper/css/navigation', () => ({}))
vi.mock('swiper/css/pagination', () => ({}))

/** The mocked catalogue endpoints, re-created for every test. */
const api = { fetchSlides: vi.fn() }

vi.mock('~/composables/apis/useCatalogApi', () => ({
  /**
   * Stands in for the catalogue API composable so the store runs without transport.
   *
   * @returns The mocked endpoints.
   */
  useCatalogApi: () => api,
}))

/**
 * Builds a slide.
 *
 * @param id The slide id.
 * @param overrides Fields that differ from the defaults.
 * @returns The slide.
 */
const slide = (id: number, overrides: Partial<SlidePublic> = {}): SlidePublic => ({
  id,
  iconName: 'school',
  brandColor: '#8b5cf6',
  imageUrl: `/images/slide-${id}.jpg`,
  demoUrl: null,
  learnMoreUrl: `/products/slide-${id}`,
  productId: null,
  monthlyPrice: null,
  annualPrice: null,
  title: `Slide ${id}`,
  tagline: 'Tagline',
  description: 'Description',
  features: ['Feature one'],
  ctaText: null,
  ctaUrl: null,
  ...overrides,
})

/**
 * Mounts the slider and lets the slides load.
 *
 * @returns The wrapper.
 */
const mountSlider = async () => {
  const wrapper = await mountPage(ProductsFullSlider)
  await flushPromises()
  return wrapper
}

describe('ProductsFullSlider', () => {
  beforeEach(() => {
    stubPublicPageRuntime()
    api.fetchSlides.mockReset()
    api.fetchSlides.mockResolvedValue([slide(1), slide(2, { demoUrl: 'https://demo.test', brandColor: '#10b981' })])
  })

  it('reads the slides in the active language', async () => {
    await mountSlider()

    expect(api.fetchSlides).toHaveBeenCalledWith('en')
  })

  it('renders a slide per catalogue slide with its title, tagline and features', async () => {
    const wrapper = await mountSlider()

    const text = wrapper.text()
    expect(text).toContain('Slide 1')
    expect(text).toContain('Slide 2')
    expect(text).toContain('Tagline')
    expect(text).toContain('Feature one')
  })

  it('paints each slide in its brand colour', async () => {
    const wrapper = await mountSlider()

    const colours = wrapper.findAll('[data-testid="ui-accent"]').map((node) => (node.element as HTMLElement).style.getPropertyValue('--accent'))
    expect(colours).toEqual(expect.arrayContaining(['#8b5cf6', '#10b981']))
  })

  it('links learn more to the slide page, and offers a demo only where the slide has one', async () => {
    const wrapper = await mountSlider()

    expect(wrapper.find('[data-testid="slider-cta-1"]').attributes('href')).toBe('/en/products/slide-1')
    expect(wrapper.find('[data-testid="slider-demo-1"]').exists()).toBe(false)
    const demo = wrapper.find('[data-testid="slider-demo-2"]')
    expect(demo.attributes('href')).toBe('https://demo.test')
    expect(demo.attributes('rel')).toBe('noopener noreferrer')
  })

  it('shows the arrows only when there is more than one slide', async () => {
    const several = await mountSlider()
    expect((several.find('[data-testid="slider-prev"]').element as HTMLElement).style.display).not.toBe('none')

    api.fetchSlides.mockResolvedValue([slide(1)])
    const single = await mountSlider()
    expect((single.find('[data-testid="slider-prev"]').element as HTMLElement).style.display).toBe('none')
  })

  it('always uses the dark palette, because it sits on photographs', async () => {
    const wrapper = await mountSlider()

    expect(wrapper.find('[data-testid="products-slider"]').classes()).toContain('dark')
  })

  it('scrolls to the bottom of the section from the scroll-down button', async () => {
    const scrollTo = vi.fn()
    window.scrollTo = scrollTo as unknown as typeof window.scrollTo
    const wrapper = await mountSlider()

    await wrapper.find('[data-testid="slider-scroll"]').trigger('click')

    expect(scrollTo).toHaveBeenCalledWith(expect.objectContaining({ behavior: 'smooth' }))
  })
})
