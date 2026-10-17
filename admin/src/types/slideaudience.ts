/**
 * Who a homepage slide is shown to. The members are the strings the API sends and accepts.
 */

/** The audience of a slide. */
export enum SlideAudience {
  /** Every visitor. */
  All = 'All',
  /** Visitors who are not signed in. */
  Guest = 'Guest',
  /** Signed-in visitors. */
  Authenticated = 'Authenticated',
}
