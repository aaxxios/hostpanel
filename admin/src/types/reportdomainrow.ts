/**
 * The row shape of the domains report.
 */

import type { DomainStatus } from '@/types/domain'

/** The money figures of a domain row. */
export interface ReportDomainAmounts {
  /** Amount of the first payment. */
  firstPayment: number
  /** Amount of each renewal. */
  recurring: number
}

/** The dates of a domain row. */
export interface ReportDomainDates {
  /** Registration date. */
  registration: string
  /** Expiry date. */
  expiry: string
  /** Next due date. */
  nextDue: string
}

/** One row of the domains report. */
export interface ReportDomainRow {
  /** Domain id. */
  id: number
  /** Owning client id. */
  clientId: number
  /** Owning client name. */
  clientName: string
  /** Order that created the domain, when known. */
  orderId: number | null
  /** Registration type (register, transfer). */
  orderType: string
  /** The domain name. */
  domainName: string
  /** Amounts of the first payment and of each renewal. */
  amounts: ReportDomainAmounts
  /** Registration period in years. */
  registrationPeriod: number
  /** Registration, expiry and next due dates. */
  dates: ReportDomainDates
  /** Registrar, when set. */
  registrar: string | null
  /** Payment method, when set. */
  paymentMethod: string | null
  /** Domain status. */
  status: DomainStatus
  /** Staff notes. */
  notes: string | null
}
