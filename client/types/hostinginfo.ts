/** Nameservers and certificate of a hosting service, as `GET /api/portal/client/services/{id}/hosting-info` returns them. */
export interface HostingInfo {
  /** The nameservers the service's domain should point at. */
  nameservers: string[]
  /** The service's SSL certificate, or null when it has none. */
  ssl: {
    /** Whether the certificate is currently valid. */
    valid: boolean
    /** Who issued it. */
    issuer: string
    /** Start of its validity, as the ISO date-time the API sends; the page formats it. */
    startDate: string
    /** End of its validity, as the ISO date-time the API sends; the page formats it. */
    expiryDate: string
  } | null
}
