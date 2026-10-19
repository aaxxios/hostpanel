/**
 * The storefront content sections whose reads fail independently.
 *
 * @module types/storefrontsection
 */

/** A storefront section the store loads and keeps a failure sentence for. */
export enum StorefrontSection {
  /** The customer quotes. */
  Testimonials = 'testimonials',
  /** The partner logos. */
  Partners = 'partners',
  /** The portfolio projects. */
  Projects = 'projects',
  /** The questions and answers. */
  Faqs = 'faqs',
}
