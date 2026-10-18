/**
 * One customer quote.
 *
 * @module templates/nova/types/testimonial
 */

/**
 * One customer quote.
 *
 * No endpoint returns these today, so the array in `content.ts` is empty and
 * the section does not render. The shape is here so a future source can fill it
 * without the section being designed from scratch — and so nobody is tempted to
 * write a plausible-looking name into the markup in the meantime.
 */
export interface Testimonial {
  /** Stable key, used for the list key. */
  key: string
  /** Attributed person, as they gave it. Never a generated name. */
  name: string
  /** Role and company, optional. */
  role?: string
  /** The quote itself, verbatim. */
  quote: string
}
