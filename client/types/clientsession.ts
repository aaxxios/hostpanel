/** Shape of the data stored in the `client_session` httpOnly cookie. */
export interface ClientSession {
  /** Legacy billing numeric client ID. */
  id: number
  /** Account email. */
  email: string
  /** Display name. */
  name: string
}
