/**
 * Image vocabulary for `UiImage`.
 */

/** Closed set of fits. */
export const UiImageFit = {
  /** The `cover` option. */
  Cover: 'cover',
  /** The `contain` option. */
  Contain: 'contain',
  /** The `none` option. */
  None: 'none',
} as const

/** One of the UiImageFit values. */
export type UiImageFit = (typeof UiImageFit)[keyof typeof UiImageFit]

/** Closed set of corner radii. */
export const UiImageRounded = {
  /** The `none` option. */
  None: 'none',
  /** The `md` option. */
  Md: 'md',
  /** The `lg` option. */
  Lg: 'lg',
  /** The `full` option. */
  Full: 'full',
} as const

/** One of the UiImageRounded values. */
export type UiImageRounded = (typeof UiImageRounded)[keyof typeof UiImageRounded]
