/** A link written inside a legal section, such as the contact address of a policy. */
export interface LegalLink {
  /** Text of the link. */
  label: string
  /** Target URL; a `mailto:` address stays a plain anchor, a web address opens in a new tab. */
  href: string
}
