/**
 * Closed value sets of the storefront content area, one file for the area: the topics a FAQ entry
 * belongs to. The members are the strings the API sends and accepts.
 */

/** The category of a FAQ entry. */
export enum FaqCategory {
  /** About ordering, billing, support and how the platform works. */
  General = 'General',
  /** Hosting plans, domains and servers. */
  Products = 'Products',
}
