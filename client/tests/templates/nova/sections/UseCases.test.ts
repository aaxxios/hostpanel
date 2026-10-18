/**
 * Unit tests for UseCases: one card per audience, each with a title and a body.
 *
 * @module tests/templates/nova/sections/UseCases.test
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UseCases from '~/templates/nova/sections/UseCases.vue'
import { USE_CASES } from '~/templates/nova/content'

describe('UseCases', () => {
  it('renders one card per audience, each with a title and body', () => {
    const wrapper = mountWithKit(UseCases)
    const cards = wrapper.findAll('ul > li')

    expect(cards).toHaveLength(USE_CASES.length)
    expect(cards[0]!.find('h3').text()).toBe(USE_CASES[0]!.titleKey)
    expect(cards[0]!.text()).toContain(USE_CASES[0]!.bodyKey)
  })
})
