/**
 * Tests for the FAQ landing page: the hero with a link to every topic, the full list of
 * questions, and the structured data for search engines.
 */
import { beforeEach, describe, expect, it } from 'vitest'
import { defineComponent, h } from 'vue'
import { mountPage, registerPageComponents, stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import type { PublicPageSpies } from '~/tests/helpers/publicPage'
import FaqCategoryNav from '~/components/faq/FaqCategoryNav.vue'
import { faqs } from '~/lib/data'
import Page from '~/pages/faq/index.vue'

/** Stands in for the FAQ list section, recording the props it was given. */
const SectionsFaqStub = defineComponent({
  name: 'SectionsFAQ',
  props: { showHeader: { type: Boolean, default: true }, showCategories: { type: Boolean, default: true } },
  setup: props => () => h('div', {
    'data-testid': 'sections-faq',
    'data-header': String(props.showHeader),
    'data-categories': String(props.showCategories),
  }),
})

/** Spies of the stood-in runtime, renewed before each test. */
let spies: PublicPageSpies

describe('pages/faq/index', () => {
  beforeEach(() => {
    spies = stubPublicPageRuntime()
    registerPageComponents({ FaqCategoryNav, SectionsFAQ: SectionsFaqStub })
  })

  it('shows the hero with the badge, the title and the subtitle', async () => {
    const wrapper = await mountPage(Page)

    expect(wrapper.find('h1').text()).toBe('faq.title')
    expect(wrapper.find('[data-testid="ui-page-hero-badge"]').text()).toBe('faq.badge')
    expect(wrapper.find('[data-testid="ui-page-hero-subtitle"]').text()).toBe('faq.subtitle')
  })

  it('links to every topic page but has no "All" link', async () => {
    const wrapper = await mountPage(Page)

    for (const topic of ['general', 'products']) {
      expect(wrapper.find(`[data-testid="faq-category-${topic}"]`).attributes('href')).toBe(`/en/faq/${topic}`)
    }
    expect(wrapper.find('[data-testid="faq-category-all"]').exists()).toBe(false)
  })

  it('lists every question without its own header or tabs', async () => {
    const wrapper = await mountPage(Page)

    const list = wrapper.find('[data-testid="sections-faq"]')
    expect(list.attributes('data-header')).toBe('false')
    expect(list.attributes('data-categories')).toBe('false')
  })

  it('publishes one structured-data question per FAQ item', async () => {
    await mountPage(Page)

    expect(spies.injectSchema).toHaveBeenCalledTimes(1)
    const schema = spies.injectSchema.mock.calls[0]?.[0] as { items: Array<{ question: string }> }
    expect(schema.items).toHaveLength(faqs.length)
    expect(schema.items[0]?.question).toBe(`faq.items.${faqs[0]?.id}.question`)
  })

  it('sets the page tags for /faq', async () => {
    await mountPage(Page)

    expect(spies.useSeo).toHaveBeenCalledWith(expect.objectContaining({ title: 'seo.faq.title', path: '/faq' }))
  })
})
