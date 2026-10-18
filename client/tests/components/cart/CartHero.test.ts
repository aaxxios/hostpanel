/**
 * Unit tests for CartHero: the page title and its lead line.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import CartHero from '~/components/cart/CartHero.vue'

describe('CartHero', () => {
  it('shows the cart title as the page heading, with the lead line', () => {
    const wrapper = mountWithKit(CartHero)

    expect(wrapper.find('h1').text()).toBe('cart.title')
    expect(wrapper.text()).toContain('cart.subtitle')
  })
})
