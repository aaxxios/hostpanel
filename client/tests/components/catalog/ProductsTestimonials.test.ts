/**
 * Unit tests for ProductsTestimonials: three testimonials read from the storefront store, each
 * with its author, quotes cut to 120 characters, and the block hiding itself when there are none.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { testimonialFixture } from '~/tests/helpers/storefrontFixtures'
import ProductsTestimonials from '~/components/catalog/ProductsTestimonials.vue'
import MarketingSectionHeader from '~/components/marketing/MarketingSectionHeader.vue'

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
 * Mounts the block with the translation component stood in for, and lets the testimonials load.
 *
 * @returns The wrapper.
 */
const mountBlock = async () => {
  const wrapper = mountWithKit(ProductsTestimonials, { components: { MarketingSectionHeader }, stubs: { 'i18n-t': true } })
  await flushPromises()
  return wrapper
}

describe('ProductsTestimonials', () => {
  beforeEach(() => {
    api.fetchTestimonials.mockReset()
    api.fetchTestimonials.mockResolvedValue([1, 2, 3, 4].map(id => testimonialFixture(id)))
  })

  it('shows the first three testimonials with their author and company', async () => {
    const wrapper = await mountBlock()

    const cards = wrapper.findAll('[data-testid^="product-testimonial-"]')
    expect(cards).toHaveLength(3)
    expect(cards[0]?.text()).toContain('Example Person 1')
    expect(cards[0]?.text()).toContain('Example Co')
  })

  it('cuts a long quote to 120 characters and an ellipsis', async () => {
    api.fetchTestimonials.mockResolvedValue([testimonialFixture(1, { quote: 'x'.repeat(200) })])

    const wrapper = await mountBlock()

    const quote = wrapper.find('[data-testid^="product-testimonial-"]').text()
    expect(quote).toContain(`"${'x'.repeat(120)}..."`)
    expect(quote).not.toContain('x'.repeat(121))
  })

  it('keeps a short quote whole', async () => {
    const wrapper = await mountBlock()

    expect(wrapper.find('[data-testid^="product-testimonial-"]').text()).toContain('"Example quote 1."')
  })

  it('renders nothing when there are no testimonials', async () => {
    api.fetchTestimonials.mockResolvedValue([])

    const wrapper = await mountBlock()

    expect(wrapper.find('[data-testid="products-testimonials"]').exists()).toBe(false)
  })
})
