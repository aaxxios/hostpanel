/**
 * What a free trial promises, one item per card of the trial page.
 *
 * The member values are the last-but-one segment of the `trial.perks.<value>.title` and
 * `trial.perks.<value>.desc` locale keys, so a perk cannot be added without its copy.
 *
 * @module types/trialperk
 */

/** A promise of the free trial. */
export enum TrialPerk {
  /** No credit card is asked for. */
  NoCc = 'noCc',
  /** How long the trial lasts. */
  Duration = 'duration',
  /** Support is included during the trial. */
  Support = 'support',
}
