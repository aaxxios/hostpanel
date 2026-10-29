/** Closed set of reasons the mail relay did not accept a message, as the API sends them. */
export const EmailSendFailure = {
  /** A reason not listed here, including entries logged before the reason was recorded. */
  Unknown: 'Unknown',
  /** The relay could not be reached. */
  RelayUnreachable: 'RelayUnreachable',
  /** The relay did not answer in time. */
  Timeout: 'Timeout',
  /** The encrypted connection to the relay could not be set up. */
  SecureConnectionFailed: 'SecureConnectionFailed',
  /** The relay refused the configured credentials. */
  AuthenticationRejected: 'AuthenticationRejected',
  /** The relay refused the recipient address. */
  RecipientRejected: 'RecipientRejected',
  /** The relay refused the sender address. */
  SenderRejected: 'SenderRejected',
  /** The relay refused the message itself. */
  MessageRejected: 'MessageRejected',
} as const

/** One of the {@link EmailSendFailure} values. */
export type EmailSendFailure = (typeof EmailSendFailure)[keyof typeof EmailSendFailure]

/** Represents a single email log entry. */
export interface EmailLog {
  /** Unique identifier. */
  id: number
  /** Recipient email address. */
  to: string
  /** Email subject line. */
  subject: string
  /** UTC timestamp of the send attempt. */
  sentAt: string
  /** Whether the email was delivered successfully. */
  success: boolean
  /** Why delivery failed; null on success. */
  failure: EmailSendFailure | null
}
