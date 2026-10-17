/** One renewal reminder e-mail that was sent. */
export interface ReportDomainRenewalRow {
  /** Client name. */
  clientName: string
  /** Domain the reminder was about. */
  domain: string
  /** When it was sent. */
  sentAt: string
  /** Which reminder it was. */
  reminder: string
  /** Recipient addresses. */
  recipients: string
}

/** The domain renewal emails report. */
export interface ReportDomainRenewal {
  /** The reminders. */
  rows: ReportDomainRenewalRow[]
}
