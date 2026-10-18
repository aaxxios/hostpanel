/** One row of the account's sent-mail list, as `GET /api/portal/client/emails` returns it. */
export interface ClientEmailSummary {
  /** Message id. */
  id: string
  /** ISO 8601 timestamp the message was sent. */
  date: string
  /** Subject line. */
  subject: string
}
