/** SSL status of one domain. */
export interface ReportSslRow {
  /** The domain. */
  domainName: string
  /** Whether a certificate was found. */
  hasSsl: boolean
  /** Certificate issuer, when known. */
  issuer: string | null
  /** Certificate expiry, when known. */
  expiresAt: string | null
  /** When the status was last checked. */
  lastUpdate: string
  /** Whether the domain is active. */
  isActive: boolean
}

/** The domains of one group. */
export interface ReportSslGroup {
  /** Group name. */
  groupName: string
  /** Domains of the group. */
  rows: ReportSslRow[]
}

/** The SSL monitoring report. */
export interface ReportSsl {
  /** Domain groups. */
  groups: ReportSslGroup[]
}
