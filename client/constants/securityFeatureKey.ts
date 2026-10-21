/**
 * The protections the security section can show.
 *
 * @module constants/securityFeatureKey
 */

/** A protection of the security section. */
export enum SecurityFeatureKey {
  /** Free SSL certificate. */
  Ssl = 'ssl',
  /** Automatic backups. */
  Backups = 'backups',
  /** DDoS protection. */
  Ddos = 'ddos',
  /** Malware scanning. */
  Malware = 'malware',
  /** Firewall. */
  Firewall = 'firewall',
}
