/**
 * One domain in full, as the client area's domain detail endpoint returns it.
 *
 * @module types/domaindetail
 */

import type { DomainManagement } from '~/types/domainmanagement'
import type { DomainStatus } from '~/types/domainstatus'

/** One domain in full, as `GET /api/portal/client/domains/{id}` returns it (the backend's `MyDomainDto`). */
export interface DomainDetail {
  /** Domain primary key. */
  id: number
  /** Domain name (e.g. "example"). */
  name: string
  /** Current lifecycle status. */
  status: DomainStatus
  /** ISO 8601 registration date. */
  registeredAt: string
  /** ISO 8601 expiration date. */
  expiresAt: string
  /** ISO 8601 next renewal payment due date. */
  nextDueDate: string
  /** One-time registration cost. */
  firstPaymentAmount: number
  /** Recurring registration price. */
  recurringAmount: number
  /** Payment method label. */
  paymentMethod: string | null
  /** Registration period in years. */
  registrationPeriod: number
  /** Whether the domain is set to auto-renew at expiration. */
  autoRenew: boolean
  /** Whether the domain is locked against unauthorized transfers. */
  isLocked: boolean
  /** Whether WHOIS privacy is enabled. */
  whoisPrivacy: boolean
  /** ISO 4217 currency code the prices above are denominated in. */
  priceCurrency: string
  /** The DNS, nameserver and forwarding settings the client manages. */
  management: DomainManagement
}
