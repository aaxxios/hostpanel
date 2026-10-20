/**
 * Text-like field vocabulary shared by `UiInput`, `UiTextarea`, `UiNativeSelect` and `UiSelect`.
 *
 * Fields draw from one class set so a form of mixed controls lines up, and every colour comes
 * from the theme tokens (surface-field, border, text-*), which is what makes them follow the
 * light/dark switch.
 */

/** Input types `UiInput` renders. File inputs have their own `UiFileInput`. */
export const UiInputType = {
  /** The `text` input type. */
  Text: 'text',
  /** The `email` input type. */
  Email: 'email',
  /** The `password` input type. */
  Password: 'password',
  /** The `number` input type. */
  Number: 'number',
  /** The `url` input type. */
  Url: 'url',
  /** The `tel` input type. */
  Tel: 'tel',
  /** The `search` input type. */
  Search: 'search',
  /** The `date` input type. */
  Date: 'date',
  /** The `time` input type. */
  Time: 'time',
  /** The `datetime-local` input type. */
  DatetimeLocal: 'datetime-local',
  /** The `month` input type. */
  Month: 'month',
  /** The `color` input type. */
  Color: 'color',
} as const

/** One of the input types. */
export type UiInputType = (typeof UiInputType)[keyof typeof UiInputType]

/**
 * Input types that are not drawn as a bordered text box (a colour well keeps the browser's
 * own swatch), so the field classes are skipped for them.
 */
export const UNSTYLED_INPUT_TYPES: readonly UiInputType[] = ['color']

/**
 * Width is applied through `:where()` so it has zero specificity: a screen's own `w-24` or
 * `flex-1` always wins over the kit default instead of racing it.
 */
export const fieldWidthClasses = '[:where(&)]:w-full'

/** Border, background, text and focus classes of a text-like field. */
export const fieldBaseClasses =
  'bg-surface-field border border-border rounded-[10px] text-text-primary placeholder:text-text-muted outline-none transition-colors duration-200 focus-visible:border-primary-500/60 focus-visible:ring-2 focus-visible:ring-primary-500/30 disabled:opacity-50 disabled:cursor-not-allowed read-only:cursor-default'

/** Extra classes of a field flagged invalid (the server refused its value). */
export const fieldInvalidClasses = 'border-status-red/60 focus-visible:border-status-red focus-visible:ring-status-red/30'
