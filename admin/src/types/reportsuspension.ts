/** One suspended service. */
export interface ReportSuspensionRow {
  /** Service id. */
  serviceId: number
  /** Client name. */
  clientName: string
  /** Product name. */
  productName: string
  /** Domain, when the service has one. */
  domain: string | null
  /** Next due date, when set. */
  nextDueDate: string | null
  /** Why it was suspended, when recorded. */
  suspendReason: string | null
}
