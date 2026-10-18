/**
 * Card padding vocabulary for `UiCard`.
 */

/** Closed set of inner paddings. */
export const UiCardPadding = {
  /** The `none` option. */
  None: 'none',
  /** The `sm` option. */
  Sm: 'sm',
  /** The `md` option. */
  Md: 'md',
  /** The `lg` option. */
  Lg: 'lg',
} as const

/** One of the UiCardPadding values. */
export type UiCardPadding = (typeof UiCardPadding)[keyof typeof UiCardPadding]
