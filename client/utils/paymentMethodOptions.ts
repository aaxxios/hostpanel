/**
 * Turning the payment gateways into select options.
 *
 * The domain renewal form and the domain transfer form offer the same list, labelled the same
 * way, so the mapping is shared.
 *
 * @module utils/paymentMethodOptions
 */

import type { GatewayMethod } from '~/types/gatewaymethod'

/** One choice of a select list. */
export interface PaymentMethodOption {
  /** Text shown to the reader. */
  label: string
  /** The gateway module name the order endpoints expect. */
  value: string
}

/**
 * Maps gateways to select options.
 *
 * The label is the gateway's display name (its module name when it has none); the value is the
 * module name the order endpoints expect.
 *
 * @param methods - The gateways; an absent list reads as none.
 * @returns The options, in the order the API returned the gateways.
 */
export const toPaymentMethodOptions = (methods: GatewayMethod[] | null | undefined): PaymentMethodOption[] =>
  (methods ?? []).map((method) => ({
    label: method.displayName || method.module,
    value: method.module
  }))
