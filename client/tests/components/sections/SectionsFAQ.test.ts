/**
 * Unit tests for SectionsFAQ: the category tabs and filtering, the preview limit, the header and
 * the contact card.
 */
import { beforeEach, describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import SectionsFAQ from '~/components/sections/SectionsFAQ.vue'
import MarketingSectionHeader from '~/components/marketing/MarketingSectionHeader.vue'
import { faqs } from '~/lib/data'
import { FaqCategory } from '~/types/faqcategory'

/**
 * Mounts the section.
 *
 * @param props Props of the section.
 * @returns The wrapper.
 */
const mountFaq = (props: Record<string, unknown> = {}) =>
  mountWithKit(SectionsFAQ, { props, components: { MarketingSectionHeader } })

/**
 * Counts the questions on screen.
 *
 * @param wrapper The mounted section.
 * @returns How many accordion headers there are.
 */
const questionCount = (wrapper: ReturnType<typeof mountFaq>): number => wrapper.findAll('[data-testid^="ui-accordion-toggle-"]').length

describe('SectionsFAQ', () => {
  beforeEach(() => {
    stubPublicPageRuntime()
  })

  it('shows every question under the "all" tab at first, with every answer closed', () => {
    const wrapper = mountFaq()

    expect(questionCount(wrapper)).toBe(faqs.length)
    expect(wrapper.find('[data-testid="ui-accordion-toggle-0"]').attributes('aria-expanded')).toBe('false')
  })

  it('offers an all tab and one tab per topic', () => {
    const wrapper = mountFaq()

    expect(wrapper.findAll('[data-testid^="faq-category-"]')).toHaveLength(Object.values(FaqCategory).length + 1)
  })

  it('filters to a topic and opens its first question', async () => {
    const wrapper = mountFaq()

    await wrapper.find(`[data-testid="faq-category-${FaqCategory.Products}"]`).trigger('click')

    expect(questionCount(wrapper)).toBe(faqs.filter((faq) => faq.category === FaqCategory.Products).length)
    expect(wrapper.find('[data-testid="ui-accordion-toggle-0"]').attributes('aria-expanded')).toBe('true')
  })

  it('cuts the list to the limit and offers the full FAQ page', () => {
    const wrapper = mountFaq({ limit: 3 })

    expect(questionCount(wrapper)).toBe(3)
    expect(wrapper.find('[data-testid="faq-view-all"]').attributes('href')).toBe('/en/faq')
  })

  it('has no view-all link without a limit', () => {
    expect(mountFaq().find('[data-testid="faq-view-all"]').exists()).toBe(false)
  })

  it('hides the tabs and the header when asked to', () => {
    const wrapper = mountFaq({ showCategories: false, showHeader: false })

    expect(wrapper.find('[data-testid="faq-category-all"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="section-header"]').exists()).toBe(false)
  })

  it('links the contact card to the contact page', () => {
    expect(mountFaq().find('[data-testid="faq-contact-button"]').attributes('href')).toBe('/en/contact')
  })
})
