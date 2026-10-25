/**
 * Unit tests for GatewayOption: the label, the icon and description chosen by the kind of the
 * gateway, the selected state and the choice it reports.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import GatewayOption from '~/components/checkout/GatewayOption.vue'
import { PAYMENT_MODULE_BANK_TRANSFER, PAYMENT_MODULE_INECOBANK, PAYMENT_MODULE_STRIPE } from '~/utils/paymentModules'

/**
 * Mounts a row for one gateway.
 *
 * @param module - The gateway module name.
 * @param displayName - Its display name.
 * @param modelValue - The chosen module.
 * @returns The wrapper.
 */
const mountOption = (module: string, displayName: string, modelValue?: string) =>
  mountWithKit(GatewayOption, {
    props: { method: { module, displayName }, name: 'gw', modelValue },
    attrs: { testid: 'gw-row' },
  })

describe('GatewayOption', () => {
  it('shows the display name with the card description for a card gateway', () => {
    const wrapper = mountOption(PAYMENT_MODULE_INECOBANK, 'Bank Card (Inecobank)')

    expect(wrapper.text()).toContain('Bank Card (Inecobank)')
    expect(wrapper.text()).toContain('checkout.gatewayKinds.card')
    expect(wrapper.find('[data-testid="ui-tile"]').exists()).toBe(true)
  })

  it('describes a bank transfer as such', () => {
    const wrapper = mountOption(PAYMENT_MODULE_BANK_TRANSFER, 'Bank transfer')

    expect(wrapper.text()).toContain('checkout.gatewayKinds.bankTransfer')
  })

  it('shows no description for a gateway of an unknown kind and falls back to its module name', () => {
    const wrapper = mountOption('crypto', '')

    expect(wrapper.text()).toBe('crypto')
    expect(wrapper.find('[data-testid="ui-tile"]').exists()).toBe(true)
  })

  it('marks the chosen gateway', () => {
    const on = mountOption(PAYMENT_MODULE_STRIPE, 'Card', PAYMENT_MODULE_STRIPE)
    const off = mountOption(PAYMENT_MODULE_STRIPE, 'Card', PAYMENT_MODULE_BANK_TRANSFER)

    expect(on.find('[data-testid="gw-row"]').attributes('data-state')).toBe('checked')
    expect(off.find('[data-testid="gw-row"]').attributes('data-state')).toBe('unchecked')
  })

  it('reports the module of the gateway that was chosen', async () => {
    const wrapper = mountOption(PAYMENT_MODULE_STRIPE, 'Card')

    await wrapper.find('[data-testid="gw-row-input"]').setValue(true)

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual([PAYMENT_MODULE_STRIPE])
  })
})
