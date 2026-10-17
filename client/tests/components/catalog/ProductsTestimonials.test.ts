/**
 * Unit tests for ProductsTestimonials: three testimonials, each with its author, and quotes cut to
 * 120 characters.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import ProductsTestimonials from '~/components/catalog/ProductsTestimonials.vue'
import MarketingSectionHeader from '~/components/marketing/MarketingSectionHeader.vue'
import { testimonials } from '~/lib/data'

/** Mounts the block with the translation component stood in for. */
const mountBlock = () => mountWithKit(ProductsTestimonials, { components: { MarketingSectionHeader }, stubs: { 'i18n-t': true } })

describe('ProductsTestimonials', () => {
  afterEach(() => {
    vi.stubGlobal('useI18n', () => ({ t: (key: string) => key, te: () => false, locale: ref('en') }))
  })

  it('shows the first three testimonials with their author and company', () => {
    const wrapper = mountBlock()

    const cards = wrapper.findAll('[data-testid^="product-testimonial-"]')
    expect(cards).toHaveLength(3)
    expect(cards[0]?.text()).toContain(testimonials[0]?.name)
    expect(cards[0]?.text()).toContain(testimonials[0]?.company)
  })

  it('cuts a long quote to 120 characters and an ellipsis', () => {
    vi.stubGlobal('useI18n', () => ({ t: () => 'x'.repeat(200), te: () => false, locale: ref('en') }))

    const wrapper = mountBlock()

    const quote = wrapper.find('[data-testid^="product-testimonial-"]').text()
    expect(quote).toContain(`"${'x'.repeat(120)}..."`)
    expect(quote).not.toContain('x'.repeat(121))
  })

  it('keeps a short quote whole', () => {
    const wrapper = mountBlock()

    expect(wrapper.find('[data-testid^="product-testimonial-"]').text()).toContain(`"testimonials.items.${testimonials[0]?.id}.text"`)
  })
})
