/**
 * Unit tests for `constants/paymentGatewayKind.ts`: every module maps to a kind, every kind has
 * an icon, and an unknown module is the generic kind.
 */
import { describe, expect, it } from 'vitest'
import {
  PAYMENT_GATEWAY_KIND_DESCRIPTION_KEY,
  PAYMENT_GATEWAY_KIND_ICON,
  paymentGatewayKind
} from '~/constants/paymentGatewayKind'
import { PaymentGatewayKind } from '~/types/paymentgatewaykind'
import { PAYMENT_MODULE_BANK_TRANSFER, PAYMENT_MODULE_INECOBANK, PAYMENT_MODULE_STRIPE } from '~/utils/paymentModules'

describe('paymentGatewayKind', () => {
  it('maps the card gateways to the card kind', () => {
    expect(paymentGatewayKind(PAYMENT_MODULE_STRIPE)).toBe(PaymentGatewayKind.Card)
    expect(paymentGatewayKind(PAYMENT_MODULE_INECOBANK)).toBe(PaymentGatewayKind.Card)
  })

  it('maps the manual transfer to the bank transfer kind', () => {
    expect(paymentGatewayKind(PAYMENT_MODULE_BANK_TRANSFER)).toBe(PaymentGatewayKind.BankTransfer)
  })

  it('reads an unknown module as the generic kind', () => {
    expect(paymentGatewayKind('crypto')).toBe(PaymentGatewayKind.Other)
  })

  it('gives every kind an icon, and a description to every kind but the generic one', () => {
    for (const kind of Object.values(PaymentGatewayKind)) {
      expect(PAYMENT_GATEWAY_KIND_ICON[kind]).toBeTruthy()
    }
    expect(PAYMENT_GATEWAY_KIND_DESCRIPTION_KEY[PaymentGatewayKind.Other]).toBeNull()
    expect(PAYMENT_GATEWAY_KIND_DESCRIPTION_KEY[PaymentGatewayKind.Card]).toBe('checkout.gatewayKinds.card')
  })
})
