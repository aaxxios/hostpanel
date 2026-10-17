/** One nameserver row the domains endpoint returns. */
export interface DomainNameserver {
  /** Nameserver record primary key. */
  id: number
  /** Nameserver hostname (e.g. "ns1.example.com"). */
  host: string
}
