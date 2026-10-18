/** One message the system sent to the account, as `GET /api/portal/client/emails/{id}` returns it. */
export interface ClientEmail {
  /** Message id. */
  id: string
  /** ISO 8601 timestamp the message was sent. */
  date: string
  /** Subject line. */
  subject: string
  /** Message body, as the HTML the system sent. */
  message: string
}
