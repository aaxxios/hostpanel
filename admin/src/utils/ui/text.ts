/**
 * Typography vocabulary for `UiText`, `UiHeading` and `UiLabel`.
 *
 * Each set is closed. `plain` is the compatibility member: it adds no classes, so a screen that
 * still passes its own typography classes renders exactly as before while screens migrate to the
 * named looks.
 */

/** Closed set of text colours. */
export const UiTextTone = {
  Plain: 'plain',
  Primary: 'primary',
  Secondary: 'secondary',
  Muted: 'muted',
  Success: 'success',
  Warning: 'warning',
  Danger: 'danger',
} as const

/** One of the text colours. */
export type UiTextTone = (typeof UiTextTone)[keyof typeof UiTextTone]

/** Text colour classes. */
export const textToneClasses: Record<UiTextTone, string> = {
  plain: '',
  primary: 'text-text-primary',
  secondary: 'text-text-secondary',
  muted: 'text-text-muted',
  success: 'text-status-green',
  warning: 'text-status-yellow',
  danger: 'text-status-red',
}

/** Closed set of text sizes. */
export const UiTextSize = {
  Plain: 'plain',
  Xs: 'xs',
  Sm: 'sm',
  Md: 'md',
  Lg: 'lg',
} as const

/** One of the text sizes. */
export type UiTextSize = (typeof UiTextSize)[keyof typeof UiTextSize]

/** Text size classes. */
export const textSizeClasses: Record<UiTextSize, string> = {
  plain: '',
  xs: 'text-[0.72rem]',
  sm: 'text-[0.78rem]',
  md: 'text-[0.82rem]',
  lg: 'text-[0.9rem]',
}

/** Closed set of heading looks. */
export const UiHeadingVariant = {
  Plain: 'plain',
  Page: 'page',
  Section: 'section',
  Card: 'card',
} as const

/** One of the heading looks. */
export type UiHeadingVariant = (typeof UiHeadingVariant)[keyof typeof UiHeadingVariant]

/** Heading classes per look. */
export const headingVariantClasses: Record<UiHeadingVariant, string> = {
  plain: '',
  page: 'font-display text-[1.75rem] font-bold text-text-primary tracking-tight leading-none',
  section: 'font-display text-[1.1rem] font-bold text-text-primary',
  card: 'font-display text-[1rem] font-semibold text-text-primary',
}

/** Closed set of label looks. */
export const UiLabelVariant = {
  Plain: 'plain',
  Field: 'field',
  Inline: 'inline',
} as const

/** One of the label looks. */
export type UiLabelVariant = (typeof UiLabelVariant)[keyof typeof UiLabelVariant]

/** Label classes per look: `field` is the caption above a control, `inline` wraps a checkbox or radio. */
export const labelVariantClasses: Record<UiLabelVariant, string> = {
  plain: '',
  field: 'block text-[0.72rem] font-semibold uppercase tracking-[0.08em] text-text-muted',
  inline: 'flex items-center gap-2 cursor-pointer text-[0.82rem] text-text-secondary',
}
