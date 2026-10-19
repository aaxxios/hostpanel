/**
 * The colour schemes an operator can pick for the admin panel.
 */

/** The operator's theme choice, as stored in the browser. */
export enum AdminTheme {
  /** Always the light palette. */
  Light = 'light',
  /** Always the dark palette. */
  Dark = 'dark',
  /** Follow the operating system's `prefers-color-scheme`. */
  System = 'system',
}
