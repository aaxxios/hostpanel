/**
 * Which kind each payment gateway is, and what a kind looks like in a picker.
 *
 * A kind is looked up by the gateway's module name; an unknown module is {@link PaymentGatewayKind.Other}
 * so a newly installed gateway still renders, with the generic icon and no description.
 *
 * @module constants/paymentGatewayKind
 */

import { Banknote, CreditCard, Landmark } from 'lucide-vue-next'
import type { Component } from 'vue'
import { PaymentGatewayKind } from '~/types/paymentgatewaykind'
import { PAYMENT_MODULE_BANK_TRANSFER, PAYMENT_MODULE_INECOBANK, PAYMENT_MODULE_STRIPE } from '~/utils/paymentModules'

/** Kind of each gateway module the portal knows by name. */
export const PAYMENT_GATEWAY_KIND_BY_MODULE: Readonly<Record<string, PaymentGatewayKind>> = {
  [PAYMENT_MODULE_STRIPE]: PaymentGatewayKind.Card,
  [PAYMENT_MODULE_INECOBANK]: PaymentGatewayKind.Card,
  [PAYMENT_MODULE_BANK_TRANSFER]: PaymentGatewayKind.BankTransfer
}

/** Icon drawn for each kind. */
export const PAYMENT_GATEWAY_KIND_ICON: Readonly<Record<PaymentGatewayKind, Component>> = {
  [PaymentGatewayKind.Card]: CreditCard,
  [PaymentGatewayKind.BankTransfer]: Landmark,
  [PaymentGatewayKind.Other]: Banknote
}

/** i18n key of each kind's short description; `Other` has none. */
export const PAYMENT_GATEWAY_KIND_DESCRIPTION_KEY: Readonly<Record<PaymentGatewayKind, string | null>> = {
  [PaymentGatewayKind.Card]: 'checkout.gatewayKinds.card',
  [PaymentGatewayKind.BankTransfer]: 'checkout.gatewayKinds.bankTransfer',
  [PaymentGatewayKind.Other]: null
}

/**
 * The kind of a gateway.
 *
 * @param module - The gateway module name as the API sent it.
 * @returns Its kind, or {@link PaymentGatewayKind.Other} when the module is not known by name.
 */
export const paymentGatewayKind = (module: string): PaymentGatewayKind =>
  PAYMENT_GATEWAY_KIND_BY_MODULE[module] ?? PaymentGatewayKind.Other
