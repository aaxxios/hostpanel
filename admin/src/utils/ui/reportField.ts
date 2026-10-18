/**
 * Field width vocabulary for `ReportField`.
 */

/** Closed set of field widths. */
export const ReportFieldWidth = {
  /** The `sm` option. */
  Sm: 'sm',
  /** The `md` option. */
  Md: 'md',
  /** The `lg` option. */
  Lg: 'lg',
  /** The `grow` option. */
  Grow: 'grow',
} as const

/** One of the ReportFieldWidth values. */
export type ReportFieldWidth = (typeof ReportFieldWidth)[keyof typeof ReportFieldWidth]
