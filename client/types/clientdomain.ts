/**
 * One domain on the client's account, as the client area's domain list returns it.
 *
 * @module types/clientdomain
 */

import type { DomainManagement } from '~/types/domainmanagement'
import type { DomainStatus } from '~/types/domainstatus'

/**
 * One domain on the client's account, as `GET /api/portal/client/domains` returns it (backend
 * `MyDomainDto`). The registrar reference, transfer code and staff notes are not part of it; the
 * transfer code comes only from its own request.
 */
export interface ClientDomain {
  /** Domain primary key. */
  id: number
  /** Full domain name (e.g. "example.com"). */
  name: string
  /** Current lifecycle status (e.g. "Active", "Expired"). */
  status: DomainStatus
  /** Domain registration date (ISO 8601 UTC). */
  registeredAt: string
  /** Domain expiration date (ISO 8601 UTC). */
  expiresAt: string
  /** Whether the domain is set to auto-renew at expiration. */
  autoRenew: boolean
  /** Whether WHOIS privacy is enabled. */
  whoisPrivacy: boolean
  /** Whether the domain is locked against unauthorized transfers. */
  isLocked: boolean
  /** One-time registration cost. */
  firstPaymentAmount: number
  /** Recurring registration price. */
  recurringAmount: number
  /** Payment method label. */
  paymentMethod: string | null
  /** ISO 4217 currency code for the price. */
  priceCurrency: string
  /** Next renewal payment due date (ISO 8601 UTC). */
  nextDueDate: string
  /** Registration period in years. */
  registrationPeriod: number
  /** The DNS, nameserver and forwarding settings the client manages. */
  management: DomainManagement
}
