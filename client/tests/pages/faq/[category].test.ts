/**
 * Tests for the FAQ topic page: the questions of one topic as an accordion with the first one
 * open, the navigation to the other topics, per-topic tags and a 404 for an unknown topic.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountPage, registerPageComponents, stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import type { PublicPageSpies } from '~/tests/helpers/publicPage'
import FaqCategoryNav from '~/components/faq/FaqCategoryNav.vue'
import { faqs } from '~/lib/data'
import { FaqCategory } from '~/types/faqcategory'
import Page from '~/pages/faq/[category].vue'

/** Spies of the stood-in runtime, renewed before each test. */
let spies: PublicPageSpies

describe('pages/faq/[category]', () => {
  beforeEach(() => {
    spies = stubPublicPageRuntime({ params: { category: FaqCategory.Products } })
    registerPageComponents({ FaqCategoryNav })
  })

  it('shows the topic heading and description from the topic copy', async () => {
    const wrapper = await mountPage(Page)

    expect(wrapper.find('h1').text()).toBe('seo.faqCategory.products.h1')
    expect(wrapper.find('[data-testid="ui-page-hero-subtitle"]').text()).toBe('seo.faqCategory.products.description')
  })

  it('puts the FAQ page and the topic in the breadcrumbs', async () => {
    const wrapper = await mountPage(Page)

    const trail = wrapper.find('nav')
    expect(trail.text()).toContain('faq.title')
    expect(trail.text()).toContain('faq.categories.products')
  })

  it('lists exactly the questions of the topic, the first one open', async () => {
    const wrapper = await mountPage(Page)

    const topicFaqs = faqs.filter(faq => faq.category === FaqCategory.Products)
    const toggles = wrapper.findAll('[data-testid^="ui-accordion-toggle-"]')
    expect(toggles).toHaveLength(topicFaqs.length)
    expect(toggles[0]?.text()).toBe(`faq.items.${topicFaqs[0]?.id}.question`)
    expect(toggles[0]?.attributes('aria-expanded')).toBe('true')
    expect(toggles[1]?.attributes('aria-expanded')).toBe('false')
    expect(wrapper.text()).toContain(`faq.items.${topicFaqs[0]?.id}.answer`)
  })

  it('highlights the current topic and offers the "All" link', async () => {
    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="faq-category-products"]').classes()).toContain('bg-primary-600')
    expect(wrapper.find('[data-testid="faq-category-all"]').attributes('href')).toBe('/en/faq')
  })

  it('links back to the landing page', async () => {
    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="faq-back-to-all"]').attributes('href')).toBe('/en/faq')
  })

  it('sets the tags and the structured data of the topic only', async () => {
    await mountPage(Page)

    expect(spies.useSeo).toHaveBeenCalledWith(expect.objectContaining({
      title: 'seo.faqCategory.products.title',
      path: '/faq/products',
    }))
    const schema = spies.injectSchema.mock.calls[0]?.[0] as { items: unknown[] }
    expect(schema.items).toHaveLength(faqs.filter(faq => faq.category === FaqCategory.Products).length)
  })

  it('raises a 404 for a topic that does not exist', async () => {
    spies = stubPublicPageRuntime({ params: { category: 'astrology' } })
    registerPageComponents({ FaqCategoryNav })
    const consoleError = vi.spyOn(console, 'error').mockImplementation(() => undefined)
    const consoleWarn = vi.spyOn(console, 'warn').mockImplementation(() => undefined)

    const wrapper = await mountPage(Page).catch(() => null)

    expect(spies.createError).toHaveBeenCalledWith(expect.objectContaining({ statusCode: 404 }))
    expect(wrapper?.find('[data-testid="faq-category-page"]').exists() ?? false).toBe(false)
    consoleError.mockRestore()
    consoleWarn.mockRestore()
  })
})
