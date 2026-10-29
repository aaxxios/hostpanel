/**
 * One DNS record on a client's domain, as the API sends it inside `DomainManagement`.
 *
 * @module types/dnsrecord
 */

import type { DnsRecordType } from '~/types/dnsrecordtype'

/** One DNS record belonging to a domain. */
export interface DnsRecord {
  /** DNS record primary key. */
  id: number
  /** Record type. */
  type: DnsRecordType
  /** Host or name (e.g. "@", "www", "mail"). */
  host: string
  /** Value (e.g. an IP address, a target hostname or a TXT string). */
  value: string
  /** Time-to-live in seconds. */
  ttl: number
  /** Priority for MX and SRV records; null for every other type. */
  priority: number | null
}
