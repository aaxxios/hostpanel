/**
 * Tests for the FAQ topic page: the questions of one topic as an accordion with the first one
 * open, the navigation to the other topics, per-topic tags and a 404 for an unknown topic.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountPage, registerPageComponents, stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import type { PublicPageSpies } from '~/tests/helpers/publicPage'
import FaqCategoryNav from '~/components/faq/FaqCategoryNav.vue'
import { faqFixture } from '~/tests/helpers/storefrontFixtures'
import { FaqCategory } from '~/types/faqcategory'
import { faqCategorySlug } from '~/utils/faqCategory'
import Page from '~/pages/faq/[category].vue'

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

/** Two general questions and two about products. */
const FAQS = [
  faqFixture(1),
  faqFixture(2),
  faqFixture(3, { category: FaqCategory.Products }),
  faqFixture(4, { category: FaqCategory.Products }),
]

/** Spies of the stood-in runtime, renewed before each test. */
let spies: PublicPageSpies

describe('pages/faq/[category]', () => {
  beforeEach(() => {
    spies = stubPublicPageRuntime({ params: { category: faqCategorySlug(FaqCategory.Products) } })
    registerPageComponents({ FaqCategoryNav })
    api.fetchFaqs.mockReset()
    api.fetchFaqs.mockResolvedValue(FAQS)
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

    const toggles = wrapper.findAll('[data-testid^="ui-accordion-toggle-"]')
    expect(toggles).toHaveLength(2)
    expect(toggles[0]?.text()).toBe('Example question 3?')
    expect(toggles[0]?.attributes('aria-expanded')).toBe('true')
    expect(toggles[1]?.attributes('aria-expanded')).toBe('false')
    expect(wrapper.text()).toContain('Example answer 3.')
    expect(wrapper.text()).not.toContain('Example question 1?')
  })

  it('shows the empty state when the topic has no questions', async () => {
    api.fetchFaqs.mockResolvedValue([])

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="faq-empty"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="faq-accordion"]').exists()).toBe(false)
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
    expect(schema.items).toHaveLength(2)
  })

  it('raises a 404 for a topic that does not exist', async () => {
    spies = stubPublicPageRuntime({ params: { category: 'astrology' } })
    registerPageComponents({ FaqCategoryNav })
    const consoleError = vi.spyOn(console, 'error').mockImplementation(() => undefined)
    const consoleWarn = vi.spyOn(console, 'warn').mockImplementation(() => undefined)

    const errorHandler = vi.fn()

    const wrapper = await mountPage(Page, { errorHandler })

    expect(spies.createError).toHaveBeenCalledWith(expect.objectContaining({ statusCode: 404 }))
    expect(errorHandler).toHaveBeenCalledWith(expect.objectContaining({ statusCode: 404 }), expect.anything(), expect.anything())
    expect(wrapper.find('[data-testid="faq-category-page"]').exists()).toBe(false)
    consoleError.mockRestore()
    consoleWarn.mockRestore()
  })

  it('declares a route validator that accepts known topics and rejects others', async () => {
    const definePageMeta = vi.fn()
    vi.stubGlobal('definePageMeta', definePageMeta)

    await mountPage(Page)

    const meta = definePageMeta.mock.calls[0]?.[0] as { validate: (to: { params: Record<string, string | string[]> }) => boolean }
    expect(meta.validate({ params: { category: 'general' } })).toBe(true)
    expect(meta.validate({ params: { category: ['products'] } })).toBe(true)
    expect(meta.validate({ params: { category: 'astrology' } })).toBe(false)
  })
})
