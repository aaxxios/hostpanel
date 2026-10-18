/**
 * What a visitor agreed to in the cookie banner.
 *
 * "No answer yet" is the absence of a value (`null` where it is read), never a member, so a
 * visitor who has not chosen can not be mistaken for one who chose the narrower option.
 */
export enum CookieConsent {
  /** Every cookie, including analytics: the tag manager may load. */
  All = 'all',
  /** Only the cookies the site needs to work. */
  Essential = 'essential'
}
