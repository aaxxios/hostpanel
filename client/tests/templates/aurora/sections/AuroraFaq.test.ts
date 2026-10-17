/**
 * Unit tests for AuroraFaq: every answer stays in the page for crawlers, the first one is open,
 * and toggling follows the accordion.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import AuroraFaq from '~/templates/aurora/sections/AuroraFaq.vue'
import { stubAuroraGlobals } from '../auroraGlobals'

/** Two questions as the locale messages deliver them. */
const MESSAGES = {
  'aurora.faq.items': [
    { q: 'How fast?', a: 'Very fast.' },
    { q: 'Is it safe?', a: 'Yes, safe.' },
  ],
}

describe('AuroraFaq', () => {
  it('renders every question and keeps every answer in the markup', () => {
    stubAuroraGlobals({ messages: MESSAGES })
    const wrapper = mountWithKit(AuroraFaq)

    expect(wrapper.text()).toContain('How fast?')
    expect(wrapper.text()).toContain('Is it safe?')
    expect(wrapper.html()).toContain('Very fast.')
    expect(wrapper.html()).toContain('Yes, safe.')
  })

  it('opens the first answer and hides the second', () => {
    stubAuroraGlobals({ messages: MESSAGES })
    const wrapper = mountWithKit(AuroraFaq)

    expect(wrapper.find('[data-testid="ui-accordion-toggle-0"]').attributes('aria-expanded')).toBe('true')
    expect(wrapper.find('[data-testid="ui-accordion-toggle-1"]').attributes('aria-expanded')).toBe('false')
  })

  it('opens another answer when its question is clicked', async () => {
    stubAuroraGlobals({ messages: MESSAGES })
    const wrapper = mountWithKit(AuroraFaq)

    await wrapper.find('[data-testid="ui-accordion-toggle-1"]').trigger('click')

    expect(wrapper.find('[data-testid="ui-accordion-toggle-1"]').attributes('aria-expanded')).toBe('true')
    expect(wrapper.find('[data-testid="ui-accordion-toggle-0"]').attributes('aria-expanded')).toBe('false')
  })

  it('renders no questions when the locale has none', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(AuroraFaq)

    expect(wrapper.find('[data-testid="ui-accordion-toggle-0"]').exists()).toBe(false)
  })
})
