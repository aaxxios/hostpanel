/**
 * Service a visitor can say a contact enquiry is about.
 *
 * The value is sent to the backend as the enquiry's `service`, and the member names the
 * `contact.form.serviceOptions.*` locale key that labels it.
 */
export enum ContactService {
  /** Web hosting plans. */
  Hosting = 'hosting',
  /** Domain registration, transfer and DNS. */
  Domains = 'domains',
  /** Virtual and dedicated servers. */
  Servers = 'servers',
  /** Invoices, payments and the client portal. */
  Billing = 'billing',
  /** Technical support. */
  Support = 'support',
  /** Anything else. */
  Other = 'other'
}
