/**
 * The body of a sign-in address change request from the account page.
 *
 * @module types/emailchangeinput
 */

/**
 * What a client sends to move their sign-in to a new address, under the backend's camelCase names.
 * Nothing changes until the link mailed to the new address is followed.
 */
export interface EmailChangeInput {
  /** The address to move to. */
  newEmail: string
  /** The account's current password, proving the person and not just the session. */
  currentPassword: string
}
