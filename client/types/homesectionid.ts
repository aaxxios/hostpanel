/**
 * Identifier of a home-page section that the navigation links to.
 *
 * @module types/homesectionid
 */

/** Anchor id of a home-page section; the section renders it as its `id`, the navigation links to `/#<id>`. */
export enum HomeSectionId {
  /** The services overview. */
  Services = 'services',
  /** The "how we work" steps. */
  Process = 'process',
}
