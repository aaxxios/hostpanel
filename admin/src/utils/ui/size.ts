/**
 * Control sizes shared by the kit.
 *
 * A closed set declared once (const object + derived union, so templates may pass `"sm"`),
 * with the literal class strings each control reads for its size.
 */

/** Closed set of control sizes. */
export const UiSize = {
  Sm: 'sm',
  Md: 'md',
  Lg: 'lg',
} as const

/** One of the control sizes. */
export type UiSize = (typeof UiSize)[keyof typeof UiSize]

/** Padding and type size of a text-like field per size. */
export const fieldSizeClasses: Record<UiSize, string> = {
  sm: 'px-2.5 py-1.5 text-[0.78rem]',
  md: 'px-3 py-2.5 text-[0.82rem]',
  lg: 'px-3.5 py-3 text-sm',
}

/** Padding, type size and radius of a button per size. */
export const buttonSizeClasses: Record<UiSize, string> = {
  sm: 'px-2.5 py-1 text-[0.75rem] rounded-lg',
  md: 'px-4 py-2 text-[0.82rem] rounded-[10px]',
  lg: 'px-6 py-3 text-[0.95rem] rounded-[10px]',
}

/** Square footprint of an icon-only button per size. */
export const iconButtonSizeClasses: Record<UiSize, string> = {
  sm: 'w-7 h-7',
  md: 'w-8 h-8',
  lg: 'w-10 h-10',
}

/** Icon edge length per size. */
export const iconSizeClasses: Record<UiSize, string> = {
  sm: 'w-3.5 h-3.5',
  md: 'w-4 h-4',
  lg: 'w-5 h-5',
}
