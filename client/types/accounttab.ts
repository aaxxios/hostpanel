/**
 * The tabs of the client account page.
 *
 * @module types/accounttab
 */

/** Which section of the account page is showing; the value is the `?tab=` query value. */
export enum AccountTab {
  /** Profile, billing address, email preferences and two-factor. */
  Details = 'details',
  /** Sub-contacts of the account. */
  Contacts = 'contacts',
  /** Team members and invitations. */
  Users = 'users',
  /** History of mail the system sent the account. */
  Emails = 'emails',
  /** Saved cards and bank accounts. */
  Payment = 'payment'
}
