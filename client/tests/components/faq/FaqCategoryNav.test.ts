/**
 * Unit tests for FaqCategoryNav: one link per FAQ topic, an optional "All" link and a highlighted
 * current topic.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import FaqCategoryNav from '~/components/faq/FaqCategoryNav.vue'
import { FaqCategory } from '~/types/faqcategory'
import { faqCategorySlug } from '~/utils/faqCategory'

describe('FaqCategoryNav', () => {
  beforeEach(() => {
    vi.stubGlobal('useLocalePath', () => (path: string) => `/en${path}`)
  })

  it.each(Object.values(FaqCategory))('links to the %s topic page', (category) => {
    const wrapper = mountWithKit(FaqCategoryNav)

    const slug = faqCategorySlug(category)
    const link = wrapper.find(`[data-testid="faq-category-${slug}"]`)
    expect(link.attributes('href')).toBe(`/en/faq/${slug}`)
    expect(link.text()).toBe(`faq.categories.${slug}`)
  })

  it('has no "All" link by default', () => {
    const wrapper = mountWithKit(FaqCategoryNav)

    expect(wrapper.find('[data-testid="faq-category-all"]').exists()).toBe(false)
  })

  it('adds an "All" link to the landing page', () => {
    const wrapper = mountWithKit(FaqCategoryNav, { props: { showAll: true } })

    const all = wrapper.find('[data-testid="faq-category-all"]')
    expect(all.attributes('href')).toBe('/en/faq')
    expect(all.text()).toBe('faq.categories.all')
  })

  it('highlights only the current topic', () => {
    const wrapper = mountWithKit(FaqCategoryNav, { props: { active: FaqCategory.Products } })

    const active = wrapper.find('[data-testid="faq-category-products"]').classes()
    const other = wrapper.find('[data-testid="faq-category-general"]').classes()
    expect(active).toContain('bg-primary-600')
    expect(other).not.toContain('bg-primary-600')
  })
})
