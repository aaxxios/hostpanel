/**
 * One message the system sent to the account, in full.
 *
 * @module types/clientemail
 */

/** One message the system sent to the account, as the backend `MyEmailLogDetailDto` sends it. */
export interface ClientEmail {
  /** Message id. */
  id: number
  /** Recipient address. */
  to: string
  /** Subject line. */
  subject: string
  /** Message body, as the HTML the system sent. */
  body: string
  /** ISO 8601 timestamp the message was sent. */
  sentAt: string
  /** Whether the message was delivered to the mail server. */
  success: boolean
}
