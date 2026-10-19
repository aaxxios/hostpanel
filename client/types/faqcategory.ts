/**
 * Topic a frequently asked question belongs to.
 *
 * A closed set whose values are the names the API serializes. The URL slug and the
 * `faq.categories.*` locale keys are the lowercase form, mapped by `utils/faqCategory.ts`.
 */
export enum FaqCategory {
  /** About ordering, billing, support and how the platform works. */
  General = 'General',
  /** Hosting plans, domains and servers. */
  Products = 'Products'
}
