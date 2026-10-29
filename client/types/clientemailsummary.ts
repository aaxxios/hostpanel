/**
 * One row of the account's sent-mail list.
 *
 * @module types/clientemailsummary
 */

/** One row of the account's sent-mail list, as the backend `MyEmailLogDto` sends it. */
export interface ClientEmailSummary {
  /** Message id. */
  id: number
  /** Recipient address. */
  to: string
  /** Subject line. */
  subject: string
  /** ISO 8601 timestamp the message was sent. */
  sentAt: string
  /** Whether the message was delivered to the mail server. */
  success: boolean
}
