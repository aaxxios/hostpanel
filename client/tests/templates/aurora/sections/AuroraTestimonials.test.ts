/**
 * Unit tests for AuroraTestimonials: the quote on show, the previous and next arrows wrapping at
 * both ends, the dots, and the empty locale.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import AuroraTestimonials from '~/templates/aurora/sections/AuroraTestimonials.vue'
import { stubAuroraGlobals } from '../auroraGlobals'

/** Two quotes as the locale messages deliver them. */
const MESSAGES = {
  'aurora.testimonials.items': [
    { quote: 'First quote', author: 'Ann', role: 'CEO' },
    { quote: 'Second quote', author: 'Bob', role: 'CTO' },
  ],
}

describe('AuroraTestimonials', () => {
  it('shows the first quote with its author and role', () => {
    stubAuroraGlobals({ messages: MESSAGES })
    const card = mountWithKit(AuroraTestimonials).find('[data-testid="testimonial-card"]')

    expect(card.text()).toContain('First quote')
    expect(card.text()).toContain('Ann')
    expect(card.text()).toContain('CEO')
  })

  it('advances with next and wraps back to the first quote', async () => {
    stubAuroraGlobals({ messages: MESSAGES })
    const wrapper = mountWithKit(AuroraTestimonials)

    await wrapper.find('[data-testid="testimonials-next"]').trigger('click')
    expect(wrapper.find('[data-testid="testimonial-card"]').text()).toContain('Second quote')

    await wrapper.find('[data-testid="testimonials-next"]').trigger('click')
    expect(wrapper.find('[data-testid="testimonial-card"]').text()).toContain('First quote')
  })

  it('wraps from the first quote to the last with previous', async () => {
    stubAuroraGlobals({ messages: MESSAGES })
    const wrapper = mountWithKit(AuroraTestimonials)

    await wrapper.find('[data-testid="testimonials-prev"]').trigger('click')

    expect(wrapper.find('[data-testid="testimonial-card"]').text()).toContain('Second quote')
  })

  it('jumps to a quote from its dot', async () => {
    stubAuroraGlobals({ messages: MESSAGES })
    const wrapper = mountWithKit(AuroraTestimonials)

    await wrapper.find('[data-testid="testimonials-dot-1"]').trigger('click')

    expect(wrapper.find('[data-testid="testimonial-card"]').text()).toContain('Second quote')
  })

  it('draws no card, and ignores the arrows, when the locale has no quotes', async () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(AuroraTestimonials)

    await wrapper.find('[data-testid="testimonials-next"]').trigger('click')

    expect(wrapper.find('[data-testid="testimonial-card"]').exists()).toBe(false)
  })
})
