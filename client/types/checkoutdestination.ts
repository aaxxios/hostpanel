/**
 * Where to send the payer once an order has been placed.
 *
 * @module types/checkoutdestination
 */

import type { CheckoutDestinationKind } from '~/types/checkoutdestinationkind'

/** Where to send the payer once an order has been placed. */
export interface CheckoutDestination {
  /** Whether {@link target} is a page of this site or an external address. */
  kind: CheckoutDestinationKind
  /** The localised path (route) or the absolute address (external) to go to. */
  target: string
}
