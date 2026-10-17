/**
 * A colour mode the page can actually be in.
 *
 * Also the colour theme of the embedded map widget, which follows the portal colour mode so a dark
 * page does not carry a glaring light map.
 *
 * @module types/colormode
 */
export enum ColorMode {
  /** The light palette. */
  Light = 'light',
  /** The dark palette. */
  Dark = 'dark'
}
