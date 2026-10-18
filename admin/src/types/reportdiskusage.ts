/** Disk and bandwidth use of one hosting account. */
export interface ReportDiskUsageRow {
  /** Client name. */
  clientName: string
  /** Account domain. */
  domain: string
  /** Disk used, formatted. */
  diskUsage: string
  /** Disk limit, formatted. */
  diskLimit: string
  /** Disk used in percent of the limit. */
  diskPercent: number
  /** Bandwidth used, formatted. */
  bwUsage: string
  /** Bandwidth limit, formatted. */
  bwLimit: string
  /** Bandwidth used in percent of the limit. */
  bwPercent: number
}

/** The accounts of one server. */
export interface ReportDiskUsageServer {
  /** Server name. */
  serverName: string
  /** Accounts of the server. */
  rows: ReportDiskUsageRow[]
}

/** The disk usage report. */
export interface ReportDiskUsage {
  /** Servers with their accounts. */
  servers: ReportDiskUsageServer[]
  /** When the figures were last fetched from the servers, if ever. */
  lastUpdated: string | null
}
