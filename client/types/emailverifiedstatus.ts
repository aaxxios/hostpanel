/**
 * The answer of `GET /api/portal/auth/email-verified`.
 */
export interface EmailVerifiedStatus {
  /** True when confirmed, false when not, null when the credential carries no answer. */
  verified: boolean | null
}
