/**
 * Unit tests for PaymentMethodRow: the label, translated type, expiry, default badge and the two emitted actions.
 */
import { describe, expect, it } from 'vitest'
import { mountAccount } from '~/tests/helpers/mountAccount'
import PaymentMethodRow from '~/components/account/PaymentMethodRow.vue'
import { PaymentMethodType } from '~/types/payment'
import type { PaymentMethod } from '~/types/payment'

/** A saved card that is not the default. */
const method: PaymentMethod = {
  id: '9', type: PaymentMethodType.Card, description: 'Visa •••• 4242', gateway_name: 'card',
  card_last_four: '4242', card_expiry: '12/30', card_type: 'visa', is_default: false
}

describe('PaymentMethodRow', () => {
  it('shows the brand with the masked number and the expiry', () => {
    const text = mountAccount(PaymentMethodRow, { props: { method } }).text()

    expect(text).toContain('Visa •••• 4242')
    expect(text).toContain('12/30')
  })

  it('shows the translated type label, never the raw type value', () => {
    const wrapper = mountAccount(PaymentMethodRow, { props: { method } })

    expect(wrapper.find('[data-testid="account-payment-type"]').text()).toBe('client.payment.type.card')
  })

  it('translates a bank account and an unknown type', () => {
    const bank = mountAccount(PaymentMethodRow, { props: { method: { ...method, type: PaymentMethodType.BankAccount } } })
    const other = mountAccount(PaymentMethodRow, { props: { method: { ...method, type: 'sepa_debit' } } })

    expect(bank.find('[data-testid="account-payment-type"]').text()).toBe('client.payment.type.bankAccount')
    expect(other.find('[data-testid="account-payment-type"]').text()).toBe('client.payment.type.other')
  })

  it('marks the default method and hides "set default" on it', () => {
    const wrapper = mountAccount(PaymentMethodRow, { props: { method: { ...method, is_default: true } } })

    expect(wrapper.find('[data-testid="account-payment-default-badge"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="account-payment-default-9"]').exists()).toBe(false)
  })

  it('offers "set default" and no badge on a method that is not the default', () => {
    const wrapper = mountAccount(PaymentMethodRow, { props: { method } })

    expect(wrapper.find('[data-testid="account-payment-default-badge"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="account-payment-default-9"]').exists()).toBe(true)
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
