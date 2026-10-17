/**
 * Unit tests for NovaFaq: one native disclosure per question.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import NovaFaq from '~/templates/nova/sections/NovaFaq.vue'
import { FAQ_ITEMS } from '~/templates/nova/content'

describe('NovaFaq', () => {
  it('renders a details element per FAQ item with its question and answer keys', () => {
    const wrapper = mountWithKit(NovaFaq)
    const items = wrapper.findAll('details')

    expect(items).toHaveLength(FAQ_ITEMS.length)
    expect(items[0]!.find('h3').text()).toBe(FAQ_ITEMS[0]!.questionKey)
    expect(items[0]!.text()).toContain(FAQ_ITEMS[0]!.answerKey)
  })

  it('gives every item a test id built from its key', () => {
    const wrapper = mountWithKit(NovaFaq)

    expect(wrapper.find(`[data-testid="nova-faq-${FAQ_ITEMS[0]!.key}"]`).exists()).toBe(true)
  })
})
