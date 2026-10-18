/**
 * Unit tests for the checkout page: it owns only the route and renders whichever checkout
 * component the active template provides.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { defineComponent, h } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import CheckoutPage from '~/pages/checkout/index.vue'

/** The template slots the page asked for. */
const requestedSlots: string[] = []

/** Stand-in for the template's checkout component. */
const TemplateCheckout = defineComponent({
  name: 'TemplateCheckout',
  setup: () => () => h('section', { 'data-testid': 'template-checkout' }),
})

describe('checkout page', () => {
  beforeEach(() => {
    requestedSlots.length = 0
    vi.stubGlobal('useTemplate', () => ({
      /**
       * Records the requested slot and answers with the stood-in checkout.
       *
       * @param name The slot name.
       * @returns The stood-in component.
       */
      slot: (name: string) => {
        requestedSlots.push(name)
        return TemplateCheckout
      },
    }))
  })

  it('renders the checkout component of the active template', () => {
    const wrapper = mountWithKit(CheckoutPage)

    expect(requestedSlots).toEqual(['checkout'])
    expect(wrapper.find('[data-testid="template-checkout"]').exists()).toBe(true)
  })
})
