/**
 * Unit tests for AuroraFaq: the questions come from the storefront store, every answer stays in
 * the page for crawlers, the first one is open, toggling follows the accordion, and the section
 * hides itself when there are no questions. The list is cut to the home limit, and the link to the
 * full FAQ page shows only when questions were left out.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { ref } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { faqFixture } from '~/tests/helpers/storefrontFixtures'
import { HOME_FAQ_LIMIT } from '~/constants/faq'
import AuroraFaq from '~/templates/aurora/sections/AuroraFaq.vue'
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
 * Mounts the section and lets the questions load.
 *
 * @returns The wrapper.
 */
const mountSection = async () => {
  stubAuroraGlobals()
  const wrapper = mountWithKit(AuroraFaq)
  await flushPromises()
  return wrapper
}

describe('AuroraFaq', () => {
  beforeEach(() => {
    api.fetchFaqs.mockReset()
    vi.stubGlobal('useContactDetails', () => ({ email: ref(''), phone: ref(''), phoneLink: ref('') }))
    api.fetchFaqs.mockResolvedValue([
      faqFixture(1, { question: 'How fast?', answer: 'Very fast.' }),
      faqFixture(2, { question: 'Is it safe?', answer: 'Yes, safe.' }),
    ])
  })

  it('renders every question and keeps every answer in the markup', async () => {
    const wrapper = await mountSection()

    expect(wrapper.text()).toContain('How fast?')
    expect(wrapper.text()).toContain('Is it safe?')
    expect(wrapper.html()).toContain('Very fast.')
    expect(wrapper.html()).toContain('Yes, safe.')
  })

  it('opens the first answer and hides the second', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="ui-accordion-toggle-0"]').attributes('aria-expanded')).toBe('true')
    expect(wrapper.find('[data-testid="ui-accordion-toggle-1"]').attributes('aria-expanded')).toBe('false')
  })

  it('opens another answer when its question is clicked', async () => {
    const wrapper = await mountSection()

    await wrapper.find('[data-testid="ui-accordion-toggle-1"]').trigger('click')

    expect(wrapper.find('[data-testid="ui-accordion-toggle-1"]').attributes('aria-expanded')).toBe('true')
    expect(wrapper.find('[data-testid="ui-accordion-toggle-0"]').attributes('aria-expanded')).toBe('false')
  })

  it('lists at most the home limit and links to the full FAQ page when more exist', async () => {
    api.fetchFaqs.mockResolvedValue(Array.from({ length: HOME_FAQ_LIMIT + 3 }, (_, index) => faqFixture(index + 1)))

    const wrapper = await mountSection()

    expect(wrapper.findAll('[data-testid^="ui-accordion-toggle-"]')).toHaveLength(HOME_FAQ_LIMIT)
    expect(wrapper.text()).not.toContain(`Example question ${HOME_FAQ_LIMIT + 1}?`)
    expect(wrapper.find('[data-testid="faq-view-all"]').attributes('href')).toBe('/faq')
  })

  it('has no view-all link when every question fits the limit', async () => {
    api.fetchFaqs.mockResolvedValue(Array.from({ length: HOME_FAQ_LIMIT }, (_, index) => faqFixture(index + 1)))

    const wrapper = await mountSection()

    expect(wrapper.findAll('[data-testid^="ui-accordion-toggle-"]')).toHaveLength(HOME_FAQ_LIMIT)
    expect(wrapper.find('[data-testid="faq-view-all"]').exists()).toBe(false)
  })

  it('renders nothing when there are no questions', async () => {
    api.fetchFaqs.mockResolvedValue([])

    const wrapper = await mountSection()

    expect(wrapper.find('#faq').exists()).toBe(false)
    expect(wrapper.find('[data-testid="aurora-faq"]').exists()).toBe(false)
  })

  it('keeps the intro sticky and links the contact call to action to the contact page', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="faq-aside"]').classes()).toEqual(expect.arrayContaining(['lg:sticky', 'lg:self-start']))
    expect(wrapper.find('[data-testid="faq-contact"]').attributes('href')).toBe('/contact')
    expect(wrapper.find('[data-testid="faq-email"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="faq-phone"]').exists()).toBe(false)
  })

  it('shows the support email and phone when the operator configured them', async () => {
    vi.stubGlobal('useContactDetails', () => ({
      email: ref('help@example.com'),
      phone: ref('+1 555 0100'),
      phoneLink: ref('+15550100'),
    }))

    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="faq-email"]').attributes('href')).toBe('mailto:help@example.com')
    expect(wrapper.find('[data-testid="faq-phone"]').attributes('href')).toBe('tel:+15550100')
    expect(wrapper.find('[data-testid="faq-phone"]').text()).toBe('+1 555 0100')
  })
})
