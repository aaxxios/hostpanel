/** A client as offered by the client pickers (name already composed for display). */
export interface ClientOption {
  /** Unique client identifier. */
  id: number
  /** Display name: "First Last" with the company in brackets when there is one. */
  name: string
  /** Email address. */
  email: string
  /** Account status (Active, Inactive, Suspended, Closed). */
  status: string
}
