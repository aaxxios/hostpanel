/**
 * Whether a cancellation is pending on a service, as `GET /api/portal/client/services/{id}/cancellation-status` returns it.
 *
 * @module types/cancellationstatus
 */

import type { CancellationType } from '~/types/cancellationtype'

/** Whether a cancellation is pending on a service, as `GET /api/portal/client/services/{id}/cancellation-status` returns it. */
export interface CancellationStatus {
  /** True when a cancellation request is waiting to take effect. */
  pending: boolean
  /** Backend `CancellationType` member name, not display text. */
  type?: CancellationType
  /** The reason the customer gave. */
  reason?: string
  /** When the cancellation takes effect. */
  date?: string
}
