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

  it('hides its glows and goes flat under the classic template', () => {
    const wrapper = mountWithKit(CartHero)

    expect(wrapper.find('[data-testid="cart-hero"]').classes()).toContain('[.tpl-classic_&]:bg-classic-surface-2')
    expect(wrapper.find('[data-testid="ui-decor"]').classes()).toContain('[.tpl-classic_&]:hidden')
  })
})
