/**
 * Placeholder vocabulary for `UiSkeleton`.
 */

/** Closed set of placeholder shapes. */
export const UiSkeletonShape = {
  /** The `text` option. */
  Text: 'text',
  /** The `block` option. */
  Block: 'block',
  /** The `circle` option. */
  Circle: 'circle',
} as const

/** One of the UiSkeletonShape values. */
export type UiSkeletonShape = (typeof UiSkeletonShape)[keyof typeof UiSkeletonShape]
