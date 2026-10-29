/**
 * DNS record types, as the API names them on `DnsRecord.type`.
 *
 * @module types/dnsrecordtype
 */

/** The DNS record types a domain's zone can hold, member names verbatim as the API sends them. */
export enum DnsRecordType {
  /** IPv4 address record. */
  A = 'A',
  /** IPv6 address record. */
  AAAA = 'AAAA',
  /** Canonical name (alias) record. */
  CNAME = 'CNAME',
  /** Mail exchange record. */
  MX = 'MX',
  /** Free-text record. */
  TXT = 'TXT',
  /** Nameserver delegation record. */
  NS = 'NS',
  /** Service locator record. */
  SRV = 'SRV'
}
