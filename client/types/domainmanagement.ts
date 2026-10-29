/**
 * The DNS, nameserver and forwarding settings a client manages on a domain, as the API sends
 * them on every domain it returns to the client area.
 *
 * @module types/domainmanagement
 */

import type { DnsRecord } from '~/types/dnsrecord'
import type { DomainNameserver } from '~/types/domainnameserver'
import type { EmailForwardingRule } from '~/types/emailforwardingrule'

/** The settings of a domain the client can manage. */
export interface DomainManagement {
  /** Whether DNS management is on. */
  dnsManagement: boolean
  /** Whether email forwarding is on. */
  emailForwarding: boolean
  /** The domain's nameservers, in the order the registrar holds them. */
  nameservers: DomainNameserver[]
  /** The domain's DNS records. */
  dnsRecords: DnsRecord[]
  /** The domain's email forwarding rules. */
  emailForwardingRules: EmailForwardingRule[]
}
