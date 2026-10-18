/**
 * Tests for `utils/paymentMethodOptions.ts`: the label of a card and of any other method.
 */
import { describe, expect, it } from 'vitest'
import { toPaymentMethodOptions } from '~/utils/paymentMethodOptions'
import type { PaymentMethod } from '~/types/payment'

/**
 * Builds a payment method with the fields the mapping reads.
 *
 * @param overrides - Fields to set.
 * @returns A payment method.
 */
const method = (overrides: Partial<PaymentMethod>): PaymentMethod => ({
  id: 'pm_1',
  type: 'card',
  description: 'Visa',
  gateway_name: 'stripe',
  ...overrides
})

describe('toPaymentMethodOptions', () => {
  it('adds the last four digits to a card', () => {
    const [option] = toPaymentMethodOptions([method({ card_last_four: '4242' })])

    expect(option).toEqual({ label: 'Visa •••• 4242', value: 'stripe' })
  })

  it('shows the description alone for a method without a card', () => {
    const [option] = toPaymentMethodOptions([method({ description: 'Bank account', gateway_name: 'bank' })])

    expect(option).toEqual({ label: 'Bank account', value: 'bank' })
  })

  it.each([undefined, null])('reads %s as no methods', (value) => {
    expect(toPaymentMethodOptions(value)).toEqual([])
  })
})
