/**
 * Unit tests for LayoutCartButton: the cart icon, the item-count badge and opening the drawer.
 */
import { beforeEach, describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import LayoutCartButton from '~/components/layout/LayoutCartButton.vue'
import { useCartStore } from '~/stores/cart'
import type { CartItem } from '~/types/cartitem'

/**
 * Builds a minimal cart line.
 *
 * @param pid Product id of the line.
 * @returns A hosting line priced in USD.
 */
const line = (pid: number): CartItem => ({
  pid,
  name: `Plan ${pid}`,
  billingcycle: 'monthly',
  cycleLabel: 'Monthly',
  amount: 10,
  currency: 'USD',
})

describe('LayoutCartButton', () => {
  beforeEach(() => {
    window.localStorage.clear()
  })

  it('is an accessible button labelled with the cart title', () => {
    const wrapper = mountWithKit(LayoutCartButton)

    const button = wrapper.find('[data-testid="layout-cart-button"]')
    expect(button.element.tagName).toBe('BUTTON')
    expect(button.attributes('aria-label')).toBe('cart.title')
  })

  it('wears the header control frame', () => {
    const wrapper = mountWithKit(LayoutCartButton)

    const classes = wrapper.find('[data-testid="layout-cart-button"]').classes()
    expect(classes).toContain('h-10')
    expect(classes).toContain('rounded-lg')
    expect(classes).toContain('border-classic-border')
  })

  it('shows no badge while the cart is empty', () => {
    const wrapper = mountWithKit(LayoutCartButton)

    expect(wrapper.find('[data-testid="layout-cart-count"]').exists()).toBe(false)
  })

  it('shows the number of lines in the cart', async () => {
    const wrapper = mountWithKit(LayoutCartButton)

    useCartStore().items = [line(1), line(2)]
    await wrapper.vm.$nextTick()

    expect(wrapper.find('[data-testid="layout-cart-count"]').text()).toBe('2')
  })

  it('opens the cart drawer when clicked', async () => {
    const wrapper = mountWithKit(LayoutCartButton)

    await wrapper.find('[data-testid="layout-cart-button"]').trigger('click')

    expect(useCartStore().isOpen).toBe(true)
  })
})
