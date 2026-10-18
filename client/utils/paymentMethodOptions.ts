/**
 * Turning the client's saved payment methods into select options.
 *
 * The domain renewal form and the domain transfer form offer the same list, labelled the same
 * way, so the mapping is shared.
 *
 * @module utils/paymentMethodOptions
 */

import type { PaymentMethod } from '~/types/payment'

/** One choice of a select list. */
export interface PaymentMethodOption {
  /** Text shown to the reader. */
  label: string
  /** The gateway name the order endpoints expect. */
  value: string
}

/**
 * Maps payment methods to select options.
 *
 * A card shows its description followed by its last four digits; any other method shows its
 * description alone. The value is the gateway name.
 *
 * @param methods - The saved payment methods; an absent list reads as none.
 * @returns The options, in the order the API returned the methods.
 */
export const toPaymentMethodOptions = (methods: PaymentMethod[] | null | undefined): PaymentMethodOption[] =>
  (methods ?? []).map((method) => ({
    label: method.card_last_four ? `${method.description} •••• ${method.card_last_four}` : method.description,
    value: method.gateway_name
  }))
