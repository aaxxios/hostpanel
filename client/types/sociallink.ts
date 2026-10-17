/**
 * One social profile link of the site footer.
 *
 * @module types/sociallink
 */

/** One operator-configured social profile; a profile with no URL is never built. */
export interface SocialLink {
  /** Accessible label and tooltip, e.g. `LinkedIn`. */
  name: string
  /** Absolute profile URL. */
  href: string
  /** Icon name including the `mdi:` prefix. */
  icon: string
}
