/** One row of the clients report. */
export interface ReportClientRow {
  /** Client id. */
  id: number
  /** Given name. */
  firstName: string
  /** Family name. */
  lastName: string
  /** Company, when set. */
  companyName: string | null
  /** E-mail address. */
  email: string
  /** First address line. */
  address1: string | null
  /** Second address line. */
  address2: string | null
  /** City. */
  city: string | null
  /** State or region. */
  state: string | null
  /** Postal code. */
  postCode: string | null
  /** Country. */
  country: string | null
  /** Phone number. */
  phone: string | null
  /** Billing currency. */
  currency: string | null
  /** Credit balance in the default currency. */
  creditBalance: number
  /** Account status. */
  status: string
  /** Creation date. */
  createdAt: string
  /** Staff notes. */
  notes: string | null
}
