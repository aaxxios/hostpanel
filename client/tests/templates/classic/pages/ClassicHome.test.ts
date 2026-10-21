/**
 * Unit tests for ClassicHome: the hidden H1 and the marketing sections, in landing order.
 */
import { describe, expect, it } from 'vitest'
import { defineComponent, h } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { HOME_FAQ_LIMIT } from '~/constants/faq'
import ClassicHome from '~/templates/classic/pages/ClassicHome.vue'

/** Names of the section components the home page composes, in the order they appear. */
const SECTION_NAMES = [
  'SectionsProductsFullSlider',
  'SectionsPartners',
  'SectionsWhyChooseUs',
  'SectionsProcess',
  'SectionsTestimonials',
  'SectionsProducts',
  'SectionsFAQ',
  'SectionsCTA',
]

/**
 * Builds a stand-in section that renders its own name, so order can be read from the DOM.
 *
 * @param name The section component's registered name.
 * @returns The stub component.
 */
const sectionStub = (name: string) => defineComponent({
  name,
  props: { limit: { type: Number, default: undefined } },
  setup: (props) => () => h('section', { 'data-section': name, 'data-limit': props.limit === undefined ? undefined : String(props.limit) }),
})

describe('ClassicHome', () => {
  const components = Object.fromEntries(SECTION_NAMES.map((name) => [name, sectionStub(name)]))

  it('renders the SEO heading visually hidden', () => {
    const wrapper = mountWithKit(ClassicHome, { components })

    const title = wrapper.find('[data-testid="classic-home-title"]')
    expect(title.element.tagName).toBe('H1')
    expect(title.classes()).toContain('sr-only')
    expect(title.text()).toBe('seo.home.h1')
  })

  it('previews only the home limit of FAQ questions', () => {
    const wrapper = mountWithKit(ClassicHome, { components })

    expect(wrapper.find('[data-section="SectionsFAQ"]').attributes('data-limit')).toBe(String(HOME_FAQ_LIMIT))
  })

  it('renders every section in landing order', () => {
    const wrapper = mountWithKit(ClassicHome, { components })

    const order = wrapper.findAll('[data-section]').map((node) => node.attributes('data-section'))
    expect(order).toEqual(SECTION_NAMES)
  })
})
