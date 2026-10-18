/**
 * Topic a frequently asked question belongs to.
 *
 * A closed set: it is the tab filter of the FAQ section, the `[category]` route parameter of the
 * FAQ pages (anything else is a 404) and the suffix of the `faq.categories.*` locale keys.
 */
export enum FaqCategory {
  /** About ordering, billing, support and how the platform works. */
  General = 'general',
  /** Hosting plans, domains and servers. */
  Products = 'products'
}
