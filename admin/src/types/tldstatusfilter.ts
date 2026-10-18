/**
 * The status choices of the TLD list filter. `All` is the empty value of the dropdown.
 */

/** Which TLDs the list shows by enabled state. */
export enum TldStatusFilter {
  /** Every TLD. */
  All = '',
  /** Only TLDs available for sale. */
  Enabled = 'Enabled',
  /** Only TLDs switched off. */
  Disabled = 'Disabled',
}
