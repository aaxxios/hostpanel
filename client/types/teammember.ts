/**
 * One person with access to the client account, as `GET /api/portal/client/users` lists them.
 * Mirrors the backend `ClientUserDto` one to one (the BFF forwards it unmapped).
 */
export interface TeamMember {
  /** Identity user id. */
  userId: string
  /** Given name. */
  firstName: string
  /** Family name. */
  lastName: string
  /** Email address. */
  email: string
  /** Whether this member owns the account. */
  isOwner: boolean
  /** Granted permissions as a bit-flags integer (8191 for owners). */
  permissions: number
  /** ISO 8601 time of their last sign-in, or null if they never signed in. */
  lastLoginAt: string | null
  /** ISO 8601 time the user was linked to the account. */
  createdAt: string
}
