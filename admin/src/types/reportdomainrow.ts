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
  /** Amount of the first payment. */
  firstPaymentAmount: number
  /** Amount of each renewal. */
  recurringAmount: number
  /** Registration period in years. */
  registrationPeriod: number
  /** Registration date. */
  registrationDate: string
  /** Expiry date. */
  expiryDate: string
  /** Next due date. */
  nextDueDate: string
  /** Registrar, when set. */
  registrar: string | null
  /** Payment method, when set. */
  paymentMethod: string | null
  /** Domain status. */
  status: string
  /** Staff notes. */
  notes: string | null
}
