/**
 * The disk and bandwidth figures a hosting server last reported for a service, as the backend's
 * `ServiceUsageDto` sends them.
 *
 * @module types/serviceusage
 */

/** Disk and bandwidth use and allowance of a service, in megabytes; a limit of 0 means unlimited. */
export interface ServiceUsage {
  /** Disk space in use. */
  diskUsedMb: number
  /** Disk space allowed; 0 when unlimited. */
  diskLimitMb: number
  /** Bandwidth used this period. */
  bandwidthUsedMb: number
  /** Bandwidth allowed; 0 when unlimited. */
  bandwidthLimitMb: number
  /** When the server last reported these figures (ISO 8601). */
  updatedAt: string
}
