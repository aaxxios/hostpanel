/**
 * What the classic hosting comparison table compares, and how each row shows its cells.
 *
 * @module types/comparisonfeature
 */

/** A specification row of the comparison table. The values are also the keys of the per-plan data. */
export enum ComparisonFeature {
  /** Disk space. */
  Storage = 'storage',
  /** Monthly transfer. */
  Bandwidth = 'bandwidth',
  /** Number of websites. */
  Websites = 'websites',
  /** Number of mailboxes. */
  Email = 'email',
  /** Number of databases. */
  Databases = 'databases',
  /** Free certificate. */
  FreeSsl = 'freeSsl',
  /** Backup frequency. */
  Backups = 'backups',
  /** Support included. */
  Support = 'support',
}
