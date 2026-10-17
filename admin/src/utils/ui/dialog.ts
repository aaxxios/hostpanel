/**
 * Dialog vocabulary for `UiDialog`.
 */

/** Closed set of dialog kinds. */
export const UiDialogVariant = {
  /** The `plain` option. */
  Plain: 'plain',
  /** The `confirm` option. */
  Confirm: 'confirm',
} as const

/** One of the UiDialogVariant values. */
export type UiDialogVariant = (typeof UiDialogVariant)[keyof typeof UiDialogVariant]

/** Closed set of panel widths. */
export const UiDialogSize = {
  /** The `sm` option. */
  Sm: 'sm',
  /** The `md` option. */
  Md: 'md',
  /** The `lg` option. */
  Lg: 'lg',
  /** The `xl` option. */
  Xl: 'xl',
} as const

/** One of the UiDialogSize values. */
export type UiDialogSize = (typeof UiDialogSize)[keyof typeof UiDialogSize]
