/**
 * Button looks shared by `UiButton`, `UiIconButton` and `UiLink`.
 *
 * `plain` is the compatibility look: the control adds no colours of its own and the screen's
 * classes decide everything, which keeps screens that still paste classes rendering exactly as
 * before. Every other look is fully owned by the kit and reads the theme tokens only.
 */

/** Closed set of button looks. */
export const UiButtonVariant = {
  Plain: 'plain',
  Primary: 'primary',
  Secondary: 'secondary',
  Ghost: 'ghost',
  Danger: 'danger',
  Link: 'link',
} as const

/** One of the button looks. */
export type UiButtonVariant = (typeof UiButtonVariant)[keyof typeof UiButtonVariant]

/** Closed set of native button types. */
export const UiButtonType = {
  Button: 'button',
  Submit: 'submit',
  Reset: 'reset',
} as const

/** One of the native button types. */
export type UiButtonType = (typeof UiButtonType)[keyof typeof UiButtonType]

/** Colour classes per look. `plain` is empty on purpose. */
export const buttonVariantClasses: Record<UiButtonVariant, string> = {
  plain: '',
  primary: 'gradient-brand text-white font-semibold shadow-lg shadow-primary-500/25 hover:opacity-90',
  secondary: 'bg-surface-field border border-border text-text-secondary font-medium hover:bg-surface-hover hover:text-text-primary',
  ghost: 'text-text-secondary hover:text-text-primary hover:bg-surface-hover',
  danger: 'bg-status-red/10 border border-status-red/30 text-status-red font-medium hover:bg-status-red/20',
  link: 'text-primary-400 light:text-primary-600 hover:text-primary-500 light:hover:text-primary-700 underline-offset-2 hover:underline',
}

/** Classes every non-plain look shares (focus ring, disabled state, motion). */
export const buttonBaseClasses =
  'inline-flex items-center justify-center gap-1.5 transition-all duration-200 cursor-pointer outline-none focus-visible:ring-2 focus-visible:ring-primary-500/60 disabled:opacity-50 disabled:cursor-not-allowed'
