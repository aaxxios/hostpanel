/** The account details a guest gives at checkout; the order registers them as a new client. */
export interface GuestDetails {
  /** Given name. */
  firstname: string
  /** Family name. */
  lastname: string
  /** Email address; also the sign-in name. */
  email: string
  /** Password for the new account. */
  password: string
  /** Contact phone number. */
  phonenumber: string
}
