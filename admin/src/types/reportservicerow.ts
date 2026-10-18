/** One row of the services report. */
export interface ReportServiceRow {
  /** Service id. */
  id: number
  /** Client id. */
  clientId: number
  /** Client name. */
  clientName: string
  /** Product id. */
  productId: number
  /** Product name. */
  productName: string
  /** Domain of the service, when it has one. */
  domain: string | null
  /** Billing cycle. */
  billingCycle: string
  /** Amount of the first payment. */
  firstPaymentAmount: number
  /** Amount of each renewal. */
  recurringAmount: number
  /** Payment method, when set. */
  paymentMethod: string | null
  /** Registration date. */
  registrationDate: string
  /** Next due date, when active. */
  nextDueDate: string | null
  /** Termination date, when terminated. */
  terminatedAt: string | null
  /** Service status. */
  status: string
  /** Staff notes. */
  notes: string | null
}
