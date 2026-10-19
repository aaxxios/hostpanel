/**
 * Tone, size and focus class maps shared by the Ui* kit.
 *
 * Every class here is a complete literal: Tailwind only emits a utility it can read in the
 * source, so a class assembled from pieces (`bg-${tone}-100`) would ship no CSS. The maps are
 * keyed by the enums in `enums.ts` so a component cannot invent a tone the kit does not own.
 *
 * @module utils/ui/tones
 */

import { UiAlertVariant, UiSize, UiTone } from './enums'

/** Focus ring used by every focusable kit control; `focus-visible` so a mouse click or a programmatic focus draws nothing. */
export const FOCUS_RING = 'focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500/50 focus-visible:ring-offset-2 focus-visible:ring-offset-transparent'

/** Focus ring that also offsets from the surface, for filled buttons. */
export const FOCUS_RING_OFFSET = 'focus:outline-none focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:ring-primary-500'

/** Soft badge / chip colours per tone, light and dark. */
export const BADGE_TONE_CLASSES: Record<UiTone, string> = {
  [UiTone.Default]: 'bg-gray-100 text-gray-800 dark:bg-white/10 dark:text-gray-300',
  [UiTone.Primary]: 'bg-primary-100 text-primary-800 dark:bg-primary-500/20 dark:text-primary-400',
  [UiTone.Secondary]: 'bg-secondary-100 text-secondary-800 dark:bg-secondary-500/20 dark:text-secondary-400',
  [UiTone.Success]: 'bg-green-100 text-green-800 dark:bg-green-500/20 dark:text-green-400',
  [UiTone.Warning]: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-500/20 dark:text-yellow-400',
  [UiTone.Danger]: 'bg-red-100 text-red-800 dark:bg-red-500/20 dark:text-red-400',
  [UiTone.Info]: 'bg-blue-100 text-blue-800 dark:bg-blue-500/20 dark:text-blue-400',
}

/** Icon colour per tone, used where an icon sits on a neutral surface (stat tiles). */
export const ICON_TONE_CLASSES: Record<UiTone, string> = {
  [UiTone.Default]: 'bg-gray-100 text-gray-500 dark:bg-white/10 dark:text-gray-400',
  [UiTone.Primary]: 'bg-primary-500/10 text-primary-600 dark:text-primary-400',
  [UiTone.Secondary]: 'bg-secondary-500/10 text-secondary-600 dark:text-secondary-400',
  [UiTone.Success]: 'bg-green-500/10 text-green-600 dark:text-green-400',
  [UiTone.Warning]: 'bg-yellow-500/10 text-yellow-600 dark:text-yellow-400',
  [UiTone.Danger]: 'bg-red-500/10 text-red-600 dark:text-red-400',
  [UiTone.Info]: 'bg-blue-500/10 text-blue-600 dark:text-blue-400',
}

/** The parts of an alert banner that change with its meaning. */
export interface AlertToneClasses {
  /** Frame and background of the banner. */
  wrap: string
  /** Colour of the leading icon. */
  icon: string
  /** Colour of the bold title. */
  title: string
  /** Colour of the body text. */
  body: string
}

/** Alert banner colours per meaning, light and dark. */
export const ALERT_TONE_CLASSES: Record<UiAlertVariant, AlertToneClasses> = {
  [UiAlertVariant.Warning]: {
    wrap: 'bg-yellow-50 dark:bg-yellow-500/10 border-yellow-300 dark:border-yellow-500/30',
    icon: 'text-yellow-500 dark:text-yellow-400',
    title: 'text-yellow-800 dark:text-yellow-300',
    body: 'text-yellow-700 dark:text-yellow-400/80',
  },
  [UiAlertVariant.Error]: {
    wrap: 'bg-red-50 dark:bg-red-500/10 border-red-200 dark:border-red-500/20',
    icon: 'text-red-500 dark:text-red-400',
    title: 'text-red-700 dark:text-red-400',
    body: 'text-red-600 dark:text-red-400/80',
  },
  [UiAlertVariant.Info]: {
    wrap: 'bg-blue-50 dark:bg-blue-500/10 border-blue-200 dark:border-blue-500/20',
    icon: 'text-blue-500 dark:text-blue-400',
    title: 'text-blue-700 dark:text-blue-300',
    body: 'text-blue-600 dark:text-blue-400/80',
  },
  [UiAlertVariant.Success]: {
    wrap: 'bg-green-50 dark:bg-green-500/10 border-green-200 dark:border-green-500/20',
    icon: 'text-green-500 dark:text-green-400',
    title: 'text-green-700 dark:text-green-300',
    body: 'text-green-600 dark:text-green-400/80',
  },
}

/** Padding and type size of a text control per size step. */
export const CONTROL_SIZE_CLASSES: Record<UiSize, string> = {
  [UiSize.Sm]: 'px-3 py-1.5 text-sm',
  [UiSize.Md]: 'px-4 py-2.5 text-base',
  [UiSize.Lg]: 'px-5 py-3 text-lg',
}

/** Padding and type size of a button per size step (a touch tighter than a field). */
export const BUTTON_SIZE_CLASSES: Record<UiSize, string> = {
  [UiSize.Sm]: 'px-3 py-1.5 text-sm',
  [UiSize.Md]: 'px-4 py-2 text-base',
  [UiSize.Lg]: 'px-6 py-3 text-lg',
}

/**
 * Classes of a text field's frame for its current state.
 *
 * One definition for UiInput, UiTextarea, UiSelect and UiDropdown, so an error or disabled field
 * looks identical whichever control draws it.
 *
 * @param state The field state flags.
 * @param state.error Whether the field shows an error.
 * @param state.disabled Whether the field is disabled.
 * @param state.open Whether a popup of the field is open (select/dropdown), which keeps the focus look.
 * @returns The Tailwind classes for border, background and ring.
 */
export const fieldStateClasses = (state: { error?: boolean; disabled?: boolean; open?: boolean }): string => {
  const frame = 'block w-full border rounded-lg transition-all duration-200 text-gray-900 dark:text-white'
  let look: string
  if (state.error) {
    look = 'border-red-400 dark:border-red-500/50 focus-visible:border-red-500 focus-visible:ring-red-500/50 bg-red-50 dark:bg-red-500/5'
  } else if (state.open) {
    look = 'border-primary-500 ring-2 ring-primary-500/50 bg-white dark:bg-white/5'
  } else {
    look = 'border-gray-300 dark:border-gray-700 focus-visible:border-primary-500 focus-visible:ring-primary-500/50 bg-white dark:bg-white/5 hover:border-gray-400 dark:hover:border-gray-600'
  }
  const disabled = state.disabled ? 'bg-gray-100 dark:bg-gray-800/50 cursor-not-allowed opacity-60' : ''
  return [frame, 'focus:outline-none focus-visible:ring-2', look, disabled].filter(Boolean).join(' ')
}
