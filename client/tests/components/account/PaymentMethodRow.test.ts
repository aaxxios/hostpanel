/**
 * Unit tests for PaymentMethodRow: the description line and the two emitted actions.
 */
import { describe, expect, it } from 'vitest'
import { mountAccount } from '~/tests/helpers/mountAccount'
import PaymentMethodRow from '~/components/account/PaymentMethodRow.vue'
import type { PaymentMethod } from '~/types/payment'

/** A saved card. */
const method: PaymentMethod = {
  id: '9', type: 'card', description: 'My card', gateway_name: 'stripe', card_last_four: '4242', card_expiry: '12/30'
}

describe('PaymentMethodRow', () => {
  it('shows the description, type, last four digits and expiry', () => {
    const text = mountAccount(PaymentMethodRow, { props: { method } }).text()

    expect(text).toContain('My card')
    expect(text).toContain('card')
    expect(text).toContain('4242')
    expect(text).toContain('12/30')
  })

  it('falls back to the gateway name without a description', () => {
    const wrapper = mountAccount(PaymentMethodRow, { props: { method: { ...method, description: '' } } })

    expect(wrapper.text()).toContain('stripe')
  })

  it('shows the bank name for a bank account', () => {
    const wrapper = mountAccount(PaymentMethodRow, {
      props: { method: { id: '1', type: 'bank', description: 'Bank', gateway_name: 'sepa', bank_name: 'First Bank' } }
    })

    expect(wrapper.text()).toContain('First Bank')
  })

  it('emits the id for "set default"', async () => {
    const wrapper = mountAccount(PaymentMethodRow, { props: { method } })

    await wrapper.find('[data-testid="account-payment-default-9"]').trigger('click')

    expect(wrapper.emitted('setDefault')).toEqual([['9']])
  })

  it('emits the id for "remove"', async () => {
    const wrapper = mountAccount(PaymentMethodRow, { props: { method } })

    await wrapper.find('[data-testid="account-payment-remove-9"]').trigger('click')

    expect(wrapper.emitted('remove')).toEqual([['9']])
  })
})
