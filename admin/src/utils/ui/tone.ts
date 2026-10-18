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

/** Pill/badge classes per tone (tinted background, matching border and text). */
export const toneBadgeClasses: Record<UiTone, string> = {
  neutral: 'bg-surface-hover text-text-secondary border-border',
  primary: 'bg-primary-500/10 text-primary-400 border-primary-500/20',
  info: 'bg-primary-500/10 text-primary-400 border-primary-500/20',
  success: 'bg-status-green/10 text-status-green border-status-green/20',
  warning: 'bg-status-yellow/10 text-status-yellow border-status-yellow/20',
  danger: 'bg-status-red/10 text-status-red border-status-red/20',
}

/** Banner classes per tone (same tint as the badge, on a block surface). */
export const toneAlertClasses: Record<UiTone, string> = toneBadgeClasses

/** Foreground-only classes per tone (icons, trend text, stat values). */
export const toneTextClasses: Record<UiTone, string> = {
  neutral: 'text-text-primary',
  primary: 'text-primary-400',
  info: 'text-primary-400',
  success: 'text-status-green',
  warning: 'text-status-yellow',
  danger: 'text-status-red',
}

/** Solid fill classes per tone (confirm buttons). */
export const toneSolidClasses: Record<UiTone, string> = {
  neutral: 'bg-surface-hover text-text-primary hover:bg-border',
  primary: 'bg-primary-500 text-white hover:bg-primary-600',
  info: 'bg-primary-500 text-white hover:bg-primary-600',
  success: 'bg-status-green text-surface-base hover:opacity-90',
  warning: 'bg-status-yellow text-surface-base hover:opacity-90',
  danger: 'bg-status-red text-surface-base hover:opacity-90',
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
