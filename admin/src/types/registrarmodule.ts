/**
 * The registrar modules a TLD price can be sourced from. The members are the strings the API
 * sends and accepts.
 */

/** A registrar integration. */
export enum RegistrarModule {
  /** The Armenian registrar. */
  NameAm = 'NameAm',
  /** The international registrar. */
  Namecheap = 'Namecheap',
}
