/**
 * The registry's WHOIS record of a domain, as `GET /api/portal/client/domains/:id/whois`
 * answers with it. Mirrors the backend `WhoisDto` one to one; it is a read-only lookup, not the
 * contact the client edits (that is {@link DomainContactFields}).
 */
export interface WhoisRecord {
  /** Name of the domain registrar. */
  registrar: string
  /** Name or identifier of the registrant. */
  registrant: string
  /** ISO 8601 time the domain was created. */
  createdAt: string
  /** ISO 8601 time the domain expires. */
  expiresAt: string
}
