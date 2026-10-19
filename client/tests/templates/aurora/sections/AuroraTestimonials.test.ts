/**
 * Unit tests for AuroraTestimonials: the quote on show comes from the storefront store, the
 * previous and next arrows wrap at both ends, the dots jump, and the section hides itself when
 * there are no quotes.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { testimonialFixture } from '~/tests/helpers/storefrontFixtures'
import AuroraTestimonials from '~/templates/aurora/sections/AuroraTestimonials.vue'
import { stubAuroraGlobals } from '../auroraGlobals'

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
 * Mounts the section and lets the quotes load.
 *
 * @returns The wrapper.
 */
const mountSection = async () => {
  stubAuroraGlobals()
  const wrapper = mountWithKit(AuroraTestimonials)
  await flushPromises()
  return wrapper
}

describe('AuroraTestimonials', () => {
  beforeEach(() => {
    api.fetchTestimonials.mockReset()
    api.fetchTestimonials.mockResolvedValue([
      testimonialFixture(1, { quote: 'First quote', name: 'Ann', role: 'CEO', company: 'Example Co' }),
      testimonialFixture(2, { quote: 'Second quote', name: 'Bob', role: null, company: null }),
    ])
  })

  it('shows the first quote with its author, role and company', async () => {
    const card = (await mountSection()).find('[data-testid="testimonial-card"]')

    expect(card.text()).toContain('First quote')
    expect(card.text()).toContain('Ann')
    expect(card.text()).toContain('CEO, Example Co')
  })

  it('advances with next and wraps back to the first quote', async () => {
    const wrapper = await mountSection()

    await wrapper.find('[data-testid="testimonials-next"]').trigger('click')
    expect(wrapper.find('[data-testid="testimonial-card"]').text()).toContain('Second quote')

    await wrapper.find('[data-testid="testimonials-next"]').trigger('click')
    expect(wrapper.find('[data-testid="testimonial-card"]').text()).toContain('First quote')
  })

  it('wraps from the first quote to the last with previous', async () => {
    const wrapper = await mountSection()

    await wrapper.find('[data-testid="testimonials-prev"]').trigger('click')

    expect(wrapper.find('[data-testid="testimonial-card"]').text()).toContain('Second quote')
  })

  it('jumps to a quote from its dot', async () => {
    const wrapper = await mountSection()

    await wrapper.find('[data-testid="testimonials-dot-1"]').trigger('click')

    expect(wrapper.find('[data-testid="testimonial-card"]').text()).toContain('Second quote')
  })

  it('renders nothing, arrows included, when there are no quotes', async () => {
    api.fetchTestimonials.mockResolvedValue([])

    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="testimonial-card"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="testimonials-next"]').exists()).toBe(false)
  })
})
