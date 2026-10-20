/**
 * Semantic tones shared by the kit (badge, alert, stat tile, dialog confirm button).
 *
 * The tone vocabulary is a closed set, declared once as a const object plus a derived union:
 * `UiTone.Danger` is the named member, and a template may still pass the literal `"danger"`
 * (a TypeScript `enum` would reject the literal in a template). Every class below is a full
 * literal so Tailwind sees it; nothing is interpolated.
 */

/** Closed set of semantic tones. */
export const UiTone = {
  Neutral: 'neutral',
  Primary: 'primary',
  Info: 'info',
  Success: 'success',
  Warning: 'warning',
  Danger: 'danger',
} as const

/** One of the semantic tones. */
export type UiTone = (typeof UiTone)[keyof typeof UiTone]

/** Pill/badge classes per tone (tinted background, matching border and text; `light:` steps keep the text AA-readable on a white surface). */
export const toneBadgeClasses: Record<UiTone, string> = {
  neutral: 'bg-surface-hover text-text-secondary border-border',
  primary: 'bg-primary-500/10 text-primary-400 light:text-primary-600 border-primary-500/20',
  info: 'bg-primary-500/10 text-primary-400 light:text-primary-600 border-primary-500/20',
  success: 'bg-status-green/10 text-status-green light:text-green-700 border-status-green/20',
  warning: 'bg-status-yellow/10 text-status-yellow light:text-yellow-700 border-status-yellow/20',
  danger: 'bg-status-red/10 text-status-red light:text-red-600 border-status-red/20',
}

/** Banner classes per tone (same tint as the badge, on a block surface). */
export const toneAlertClasses: Record<UiTone, string> = toneBadgeClasses

/** Foreground-only classes per tone (icons, trend text, stat values). */
export const toneTextClasses: Record<UiTone, string> = {
  neutral: 'text-text-primary',
  primary: 'text-primary-400 light:text-primary-600',
  info: 'text-primary-400 light:text-primary-600',
  success: 'text-status-green light:text-green-700',
  warning: 'text-status-yellow light:text-yellow-700',
  danger: 'text-status-red light:text-red-600',
}

/** Solid fill classes per tone (confirm buttons); the status fills carry dark text on the bright dark-theme fill and white on the deeper light-theme fill. */
export const toneSolidClasses: Record<UiTone, string> = {
  neutral: 'bg-surface-hover text-text-primary hover:bg-border',
  primary: 'bg-primary-600 light:bg-primary-700 text-white hover:bg-primary-700 light:hover:bg-primary-800',
  info: 'bg-primary-600 light:bg-primary-700 text-white hover:bg-primary-700 light:hover:bg-primary-800',
  success: 'bg-status-green text-gray-950 light:text-white hover:opacity-90',
  warning: 'bg-status-yellow text-gray-950 light:text-white hover:opacity-90',
  danger: 'bg-status-red text-gray-950 light:text-white hover:opacity-90',
}

/** Dot classes per tone (status dots inside a badge). */
export const toneDotClasses: Record<UiTone, string> = {
  neutral: 'bg-text-muted',
  primary: 'bg-primary-500',
  info: 'bg-primary-500',
  success: 'bg-status-green',
  warning: 'bg-status-yellow',
  danger: 'bg-status-red',
}
