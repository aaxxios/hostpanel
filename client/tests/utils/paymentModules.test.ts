/**
 * Unit tests for `utils/paymentModules.ts`: the module ids are the strings the API returns.
 *
 * @module tests/utils/paymentModules.test
 */

import { describe, expect, it } from 'vitest'
import { PAYMENT_MODULE_BANK_TRANSFER, PAYMENT_MODULE_STRIPE } from '~/utils/paymentModules'

describe('payment module ids', () => {
  it('match the backend built-in module ids', () => {
    expect(PAYMENT_MODULE_STRIPE).toBe('stripe')
    expect(PAYMENT_MODULE_BANK_TRANSFER).toBe('bank_transfer')
  })
})
